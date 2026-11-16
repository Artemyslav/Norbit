using System;

namespace BookingSystem
{
    public enum BookingStatus { Confirmed, PaymentFailed }

    public sealed record BookingRequest(int EventId, string CustomerEmail, int SeatsCount, decimal PricePerSeat);
    
    public sealed record Booking(
        Guid Id, int EventId, string CustomerEmail, int SeatsCount, 
        decimal TotalAmount, DateTimeOffset CreatedAt, BookingStatus Status);

    public interface IEventRepository
    {
        bool Exists(int eventId);
        int GetAvailableSeats(int eventId);
        void ReserveSeats(int eventId, int seatsCount);
        void ReleaseSeats(int eventId, int seatsCount);
    }

    public interface IBookingRepository { void Save(Booking booking); }
    public interface IPaymentService { bool Charge(string customerEmail, decimal amount); }
    public interface IClock { DateTimeOffset UtcNow { get; } }
    public interface IBookingIdGenerator { Guid Create(); }

    public sealed class BookingService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentService _paymentService;
        private readonly IClock _clock;
        private readonly IBookingIdGenerator _idGenerator;

        public BookingService(
            IEventRepository eventRepository,
            IBookingRepository bookingRepository,
            IPaymentService paymentService,
            IClock clock,
            IBookingIdGenerator idGenerator)
        {
            _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public Booking CreateBooking(BookingRequest request)
        {
            // 1. Валидация входных данных
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.EventId <= 0) throw new ArgumentException("EventId должен быть > 0", nameof(request.EventId));
            if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains("@"))
                throw new ArgumentException("Некорректный email", nameof(request.CustomerEmail));
            if (request.SeatsCount < 1 || request.SeatsCount > 10)
                throw new ArgumentOutOfRangeException(nameof(request.SeatsCount), "Количество мест должно быть от 1 до 10");
            if (request.PricePerSeat <= 0) throw new ArgumentException("Цена должна быть > 0", nameof(request.PricePerSeat));

            // 2. Проверка доступности
            if (!_eventRepository.Exists(request.EventId))
                throw new InvalidOperationException("Мероприятие не существует");

            if (_eventRepository.GetAvailableSeats(request.EventId) < request.SeatsCount)
                throw new InvalidOperationException("Недостаточно свободных мест");

            // 3. Подготовка данных
            decimal totalAmount = request.SeatsCount * request.PricePerSeat;
            Guid bookingId = _idGenerator.Create();
            DateTimeOffset createdAt = _clock.UtcNow;

            // 4. Резервирование
            _eventRepository.ReserveSeats(request.EventId, request.SeatsCount);

            // 5. Оплата и финализация
            try
            {
                bool isPaid = _paymentService.Charge(request.CustomerEmail, totalAmount);

                if (isPaid)
                {
                    var booking = new Booking(bookingId, request.EventId, request.CustomerEmail, request.SeatsCount, totalAmount, createdAt, BookingStatus.Confirmed);
                    _bookingRepository.Save(booking);
                    return booking;
                }
                else
                {
                    _eventRepository.ReleaseSeats(request.EventId, request.SeatsCount);
                    var booking = new Booking(bookingId, request.EventId, request.CustomerEmail, request.SeatsCount, totalAmount, createdAt, BookingStatus.PaymentFailed);
                    _bookingRepository.Save(booking);
                    return booking;
                }
            }
            catch
            {
                // Если оплата выбросила исключение, освобождаем места и пробрасываем ошибку дальше
                _eventRepository.ReleaseSeats(request.EventId, request.SeatsCount);
                throw;
            }
        }
    }
}