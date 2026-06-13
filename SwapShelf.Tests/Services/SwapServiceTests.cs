using NSubstitute;
using SwapShelf.DTOs;
using SwapShelf.Models;
using SwapShelf.Repositories;
using SwapShelf.Services.Implementations;
using SwapShelf.Tests.Helpers;

namespace SwapShelf.Tests.Services
{
    public class SwapServiceTests
    {
        private readonly ISwapRepository       _swapRepo       = Substitute.For<ISwapRepository>();
        private readonly IListingRepository    _listingRepo    = Substitute.For<IListingRepository>();
        private readonly IWantedBookRepository _wantedBookRepo = Substitute.For<IWantedBookRepository>();
        private readonly SwapService _sut;

        public SwapServiceTests()
        {
            _sut = new SwapService(_swapRepo, _listingRepo, _wantedBookRepo);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_SameListingIdForBothSides_ThrowsInvalidOperationException()
        {
            var request = new SwapRequestCreate { InitiatorListingId = 5, ReceiverListingId = 5 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(1, request));
        }

        [Fact]
        public async Task CreateAsync_InitiatorDoesNotOwnListing_ThrowsUnauthorizedAccessException()
        {
            _listingRepo.GetByIdAsync(1).Returns(TestBuilders.Listing(1, userId: 99));
            _listingRepo.GetByIdAsync(2).Returns(TestBuilders.Listing(2, userId: 2));

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.CreateAsync(initiatorId: 1, new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 }));
        }

        [Fact]
        public async Task CreateAsync_SwappingWithSelf_ThrowsInvalidOperationException()
        {
            _listingRepo.GetByIdAsync(1).Returns(TestBuilders.Listing(1, userId: 1));
            _listingRepo.GetByIdAsync(2).Returns(TestBuilders.Listing(2, userId: 1)); // same owner

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(initiatorId: 1, new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 }));
        }

        [Fact]
        public async Task CreateAsync_InitiatorListingNotAvailable_ThrowsInvalidOperationException()
        {
            _listingRepo.GetByIdAsync(1).Returns(TestBuilders.Listing(1, userId: 1, status: ListingStatus.Locked));
            _listingRepo.GetByIdAsync(2).Returns(TestBuilders.Listing(2, userId: 2));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(initiatorId: 1, new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 }));
        }

        [Fact]
        public async Task CreateAsync_ReceiverListingNotAvailable_ThrowsInvalidOperationException()
        {
            _listingRepo.GetByIdAsync(1).Returns(TestBuilders.Listing(1, userId: 1));
            _listingRepo.GetByIdAsync(2).Returns(TestBuilders.Listing(2, userId: 2, status: ListingStatus.Swapped));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(initiatorId: 1, new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 }));
        }

        [Fact]
        public async Task CreateAsync_InitiatorListingAlreadyHasActiveSwap_ThrowsInvalidOperationException()
        {
            _listingRepo.GetByIdAsync(1).Returns(TestBuilders.Listing(1, userId: 1));
            _listingRepo.GetByIdAsync(2).Returns(TestBuilders.Listing(2, userId: 2));
            _swapRepo.HasActiveSwapForListingAsync(1).Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateAsync(initiatorId: 1, new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 }));
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_LocksInitiatorAndReceiverListings()
        {
            var initiatorListing = TestBuilders.Listing(1, userId: 1);
            var receiverListing  = TestBuilders.Listing(2, userId: 2);
            var fullSwap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2,
                initiatorListingId: 1, receiverListingId: 2);

            _listingRepo.GetByIdAsync(1).Returns(initiatorListing);
            _listingRepo.GetByIdAsync(2).Returns(receiverListing);
            _swapRepo.HasActiveSwapForListingAsync(1).Returns(false);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(ci => Task.FromResult(ci.Arg<Listing>()));
            _swapRepo.CreateAsync(Arg.Any<SwapRequest>()).Returns(fullSwap);
            _swapRepo.GetByIdAsync(fullSwap.Id).Returns(fullSwap);

            await _sut.CreateAsync(initiatorId: 1,
                new SwapRequestCreate { InitiatorListingId = 1, ReceiverListingId = 2 });

            await _listingRepo.Received(1).UpdateAsync(Arg.Is<Listing>(l => l.Id == 1 && l.Status == ListingStatus.Locked));
            await _listingRepo.Received(1).UpdateAsync(Arg.Is<Listing>(l => l.Id == 2 && l.Status == ListingStatus.Locked));
        }

        // ── AcceptAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task AcceptAsync_InitiatorTriesToAccept_ThrowsUnauthorizedAccessException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Pending);
            _swapRepo.GetByIdAsync(1).Returns(swap);

            // User 1 is the initiator, only the receiver (2) may accept
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.AcceptAsync(swapId: 1, userId: 1));
        }

        [Fact]
        public async Task AcceptAsync_NonPendingSwap_ThrowsInvalidOperationException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Accepted);
            _swapRepo.GetByIdAsync(1).Returns(swap);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AcceptAsync(swapId: 1, userId: 2));
        }

        [Fact]
        public async Task AcceptAsync_ReceiverAndPending_ChangesStatusToAccepted()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Pending);
            _swapRepo.GetByIdAsync(1).Returns(swap);
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            var result = await _sut.AcceptAsync(swapId: 1, userId: 2);

            Assert.Equal(SwapStatus.Accepted, result.Status);
        }

        // ── RejectAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task RejectAsync_ValidRequest_RejectsSwapAndUnlocksListings()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2,
                initiatorListingId: 1, receiverListingId: 2, status: SwapStatus.Pending);

            _swapRepo.GetByIdAsync(1).Returns(swap);
            _listingRepo.GetByIdAsync(1).Returns(swap.InitiatorListing);
            _listingRepo.GetByIdAsync(2).Returns(swap.ReceiverListing);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(ci => Task.FromResult(ci.Arg<Listing>()));
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            var result = await _sut.RejectAsync(swapId: 1, userId: 2);

            Assert.Equal(SwapStatus.Rejected, result.Status);
            await _listingRepo.Received(2).UpdateAsync(Arg.Is<Listing>(l => l.Status == ListingStatus.Available));
        }

        // ── MarkInTransitAsync ────────────────────────────────────────────────

        [Fact]
        public async Task MarkInTransitAsync_SwapNotAccepted_ThrowsInvalidOperationException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Pending);
            _swapRepo.GetByIdAsync(1).Returns(swap);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.MarkInTransitAsync(1, userId: 1));
        }

        [Fact]
        public async Task MarkInTransitAsync_AcceptedSwap_ChangesStatusToInTransit()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Accepted);
            _swapRepo.GetByIdAsync(1).Returns(swap);
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            var result = await _sut.MarkInTransitAsync(swapId: 1, userId: 1);

            Assert.Equal(SwapStatus.InTransit, result.Status);
        }

        // ── CompleteAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task CompleteAsync_SwapNotInTransit_ThrowsInvalidOperationException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Accepted);
            _swapRepo.GetByIdAsync(1).Returns(swap);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CompleteAsync(1, userId: 1));
        }

        [Fact]
        public async Task CompleteAsync_ValidRequest_MarksBothListingsAsSwapped()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2,
                initiatorListingId: 1, receiverListingId: 2, status: SwapStatus.InTransit);

            _swapRepo.GetByIdAsync(1).Returns(swap);
            _listingRepo.GetByIdAsync(1).Returns(swap.InitiatorListing);
            _listingRepo.GetByIdAsync(2).Returns(swap.ReceiverListing);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(ci => Task.FromResult(ci.Arg<Listing>()));
            _wantedBookRepo.DeleteByUserAndBookAsync(Arg.Any<int>(), Arg.Any<int>())
                           .Returns(Task.CompletedTask);
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            var result = await _sut.CompleteAsync(swapId: 1, userId: 1);

            Assert.Equal(SwapStatus.Completed, result.Status);
            await _listingRepo.Received(2).UpdateAsync(Arg.Is<Listing>(l => l.Status == ListingStatus.Swapped));
        }

        [Fact]
        public async Task CompleteAsync_ValidRequest_RemovesReceivedBookFromBothWantedLists()
        {
            // Initiator (user 1) has book A (id=10), receiver (user 2) has book B (id=20)
            var bookA = TestBuilders.Book(10, "Book A");
            var bookB = TestBuilders.Book(20, "Book B");

            var initiatorListing = TestBuilders.Listing(1, userId: 1, bookId: 10);
            initiatorListing.Book = bookA;

            var receiverListing = TestBuilders.Listing(2, userId: 2, bookId: 20);
            receiverListing.Book = bookB;

            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2,
                initiatorListingId: 1, receiverListingId: 2, status: SwapStatus.InTransit);
            swap.InitiatorListing = initiatorListing;
            swap.ReceiverListing  = receiverListing;

            _swapRepo.GetByIdAsync(1).Returns(swap);
            _listingRepo.GetByIdAsync(1).Returns(initiatorListing);
            _listingRepo.GetByIdAsync(2).Returns(receiverListing);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(ci => Task.FromResult(ci.Arg<Listing>()));
            _wantedBookRepo.DeleteByUserAndBookAsync(Arg.Any<int>(), Arg.Any<int>())
                           .Returns(Task.CompletedTask);
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            await _sut.CompleteAsync(swapId: 1, userId: 1);

            // Initiator (1) received book B (20) → remove from their wanted list
            await _wantedBookRepo.Received(1).DeleteByUserAndBookAsync(1, 20);
            // Receiver (2) received book A (10) → remove from their wanted list
            await _wantedBookRepo.Received(1).DeleteByUserAndBookAsync(2, 10);
        }

        // ── CancelAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CancelAsync_AlreadyCompleted_ThrowsInvalidOperationException()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2, status: SwapStatus.Completed);
            _swapRepo.GetByIdAsync(1).Returns(swap);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CancelAsync(1, userId: 1));
        }

        [Fact]
        public async Task CancelAsync_PendingSwap_CancelsAndUnlocksListings()
        {
            var swap = TestBuilders.SwapRequest(1, initiatorId: 1, receiverId: 2,
                initiatorListingId: 1, receiverListingId: 2, status: SwapStatus.Pending);

            _swapRepo.GetByIdAsync(1).Returns(swap);
            _listingRepo.GetByIdAsync(1).Returns(swap.InitiatorListing);
            _listingRepo.GetByIdAsync(2).Returns(swap.ReceiverListing);
            _listingRepo.UpdateAsync(Arg.Any<Listing>()).Returns(ci => Task.FromResult(ci.Arg<Listing>()));
            _swapRepo.UpdateAsync(Arg.Any<SwapRequest>()).Returns(ci => Task.FromResult(ci.Arg<SwapRequest>()));

            var result = await _sut.CancelAsync(swapId: 1, userId: 1);

            Assert.Equal(SwapStatus.Cancelled, result.Status);
            await _listingRepo.Received(2).UpdateAsync(Arg.Is<Listing>(l => l.Status == ListingStatus.Available));
        }
    }
}
