using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BookingSystem;

namespace BookingSystem.Tests
{
    [TestClass]
    public class BookingServiceTests
    {
        private FakeEventRepository _fakeEventRepo = null!;
        private FakeBookingRepository _fakeBookingRepo = null!;
        private FakePaymentService _fakePaymentService = null!;
        private FakeClock _fakeClock = null!;
        private FakeBookingIdGenerator _fakeIdGenerator = null!;
        private BookingService _bookingService = null!;

        private readonly Guid _testGuid = new Guid("11111111-1111-1111-1111-111111111111");
        private readonly DateTimeOffset _testDate = new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero);

        [TestInitialize]
        public void Setup()
        {
            _fakeEventRepo = new FakeEventRepository { ExistsResult = true, AvailableSeatsResult = 10 };
            _fakeBookingRepo = new FakeBookingRepository();
            _fakePaymentService = new FakePaymentService { ChargeResult = true };
            _fakeClock = new FakeClock { UtcNow = _testDate };
            _fakeIdGenerator = new FakeBookingIdGenerator { CreateResult = _testGuid };

            _bookingService = new BookingService(
                _fakeEventRepo, _fakeBookingRepo, _fakePaymentService, _fakeClock, _fakeIdGenerator);
        }

        [TestMethod]
        public void CreateBooking_ValidRequest_ReturnsConfirmedBooking()
        {
            // Arrange
            var request = new BookingRequest(EventId: 1, CustomerEmail: "test@test.com", SeatsCount: 2, PricePerSeat: 500m);

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual(_testGuid, result.Id);
            Assert.AreEqual(_testDate, result.CreatedAt);
            Assert.AreEqual(2, result.SeatsCount);
            Assert.AreEqual(1000m, result.TotalAmount);
            Assert.AreEqual(BookingStatus.Confirmed, result.Status);
            
            Assert.AreEqual(1, _fakeEventRepo.ReserveSeatsCallCount);
            Assert.AreEqual(1, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(1000m, _fakePaymentService.LastChargedAmount);
            Assert.AreEqual(1, _fakeBookingRepo.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_PaymentFails_ReleasesReservedSeatsAndSavesFailedBooking()
        {
            // Arrange
            _fakePaymentService.ChargeResult = false;
            var request = new BookingRequest(1, "test@test.com", 3, 100m);

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual(BookingStatus.PaymentFailed, result.Status);
            Assert.AreEqual(1, _fakeEventRepo.ReserveSeatsCallCount);
            Assert.AreEqual(1, _fakeEventRepo.ReleaseSeatsCallCount); 
            Assert.AreEqual(1, _fakeBookingRepo.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_PaymentThrowsException_ReleasesSeatsAndThrowsAndDoesNotSave()
        {
            // Arrange
            _fakePaymentService.ChargeException = new InvalidOperationException("Сбой шлюза");
            var request = new BookingRequest(1, "test@test.com", 1, 100m);

            // Act & Assert (Универсальный try-catch подход)
            try
            {
                _bookingService.CreateBooking(request);
                Assert.Fail("Ожидалось исключение InvalidOperationException, но метод завершился успешно.");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Сбой шлюза", ex.Message);
            }
            
            Assert.AreEqual(1, _fakeEventRepo.ReleaseSeatsCallCount); 
            Assert.AreEqual(0, _fakeBookingRepo.SaveCallCount); 
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(11)]
        [DataRow(100)]
        public void CreateBooking_InvalidSeatsCount_ThrowsArgumentOutOfRangeException(int seatsCount)
        {
            // Arrange
            var request = new BookingRequest(1, "test@test.com", seatsCount, 100m);

            // Act & Assert
            try
            {
                _bookingService.CreateBooking(request);
                Assert.Fail("Ожидалось исключение ArgumentOutOfRangeException.");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Исключение поймано
            }
            
            Assert.AreEqual(0, _fakeEventRepo.ReserveSeatsCallCount);
            Assert.AreEqual(0, _fakePaymentService.ChargeCallCount);
            Assert.AreEqual(0, _fakeBookingRepo.SaveCallCount);
        }

        [TestMethod]
        public void CreateBooking_EventDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            _fakeEventRepo.ExistsResult = false;
            var request = new BookingRequest(1, "test@test.com", 1, 100m);

            // Act & Assert
            try
            {
                _bookingService.CreateBooking(request);
                Assert.Fail("Ожидалось исключение InvalidOperationException.");
            }
            catch (InvalidOperationException)
            {
                // Исключение поймано
            }
            
            Assert.AreEqual(0, _fakeEventRepo.ReserveSeatsCallCount);
        }

        [TestMethod]
        public void CreateBooking_NotEnoughSeats_ThrowsInvalidOperationException()
        {
            // Arrange
            _fakeEventRepo.AvailableSeatsResult = 2; 
            var request = new BookingRequest(1, "test@test.com", 3, 100m);

            // Act & Assert
            try
            {
                _bookingService.CreateBooking(request);
                Assert.Fail("Ожидалось исключение InvalidOperationException.");
            }
            catch (InvalidOperationException)
            {
                // Исключение поймано
            }
            
            Assert.AreEqual(0, _fakeEventRepo.ReserveSeatsCallCount);
        }

        [TestMethod]
        [DataRow(1, "100", "100")]
        [DataRow(2, "500", "1000")]
        [DataRow(3, "1250", "3750")]
        public void CreateBooking_CalculatesTotalAmountCorrectly(int seats, string priceStr, string expectedStr)
        {
            // Arrange
            decimal price = decimal.Parse(priceStr);
            decimal expectedTotal = decimal.Parse(expectedStr);
            var request = new BookingRequest(1, "test@test.com", seats, price);

            // Act
            var result = _bookingService.CreateBooking(request);

            // Assert
            Assert.AreEqual(expectedTotal, result.TotalAmount);
        }
    }

    // FAKE Классы

    public class FakeEventRepository : IEventRepository
    {
        public bool ExistsResult { get; set; }
        public int AvailableSeatsResult { get; set; }
        public int ReserveSeatsCallCount { get; private set; }
        public int ReleaseSeatsCallCount { get; private set; }

        public bool Exists(int eventId) => ExistsResult;
        public int GetAvailableSeats(int eventId) => AvailableSeatsResult;
        public void ReserveSeats(int eventId, int seatsCount) => ReserveSeatsCallCount++;
        public void ReleaseSeats(int eventId, int seatsCount) => ReleaseSeatsCallCount++;
    }

    public class FakeBookingRepository : IBookingRepository
    {
        public int SaveCallCount { get; private set; }
        public Booking? LastSavedBooking { get; private set; }

        public void Save(Booking booking)
        {
            SaveCallCount++;
            LastSavedBooking = booking;
        }
    }

    public class FakePaymentService : IPaymentService
    {
        public bool ChargeResult { get; set; } = true;
        public Exception? ChargeException { get; set; }
        public int ChargeCallCount { get; private set; }
        public decimal LastChargedAmount { get; private set; }

        public bool Charge(string customerEmail, decimal amount)
        {
            ChargeCallCount++;
            LastChargedAmount = amount;
            if (ChargeException != null) throw ChargeException;
            return ChargeResult;
        }
    }

    public class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }

    public class FakeBookingIdGenerator : IBookingIdGenerator
    {
        public Guid CreateResult { get; set; }
        public Guid Create() => CreateResult;
    }
}