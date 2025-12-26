using Xunit;
using SharpCards; // Namespace twojej głównej aplikacji

namespace SharpCards.Tests
{
    public class FlashcardTests
    {
        [Fact]
        public void NewCard_ShouldBe_ReadyForReview_Immediately()
        {
            // Arrange
            var card = new Flashcard 
            { 
                Box = 1, 
                LastReviewDate = DateTime.MinValue 
            };

            // Act
            var isReady = card.IsReadyToReview();

            // Assert
            Assert.True(isReady, "Nowa karta powinna być gotowa do nauki od razu.");
        }

        [Fact]
        public void CardInBox2_ShouldNotBe_Ready_AfterOneDay()
        {
            // Arrange (Pudełko 2 wymaga 3 dni przerwy)
            var card = new Flashcard 
            { 
                Box = 2, 
                LastReviewDate = DateTime.Now.AddDays(-1) // Powtórzono wczoraj
            };

            // Act
            var isReady = card.IsReadyToReview();

            // Assert
            Assert.False(isReady, "Karta z pudełka 2 nie powinna być dostępna po 1 dniu.");
        }

        [Fact]
        public void CardInBox2_ShouldBe_Ready_AfterThreeDays()
        {
            // Arrange
            var card = new Flashcard 
            { 
                Box = 2, 
                LastReviewDate = DateTime.Now.AddDays(-4) // 4 dni temu
            };

            // Act
            var isReady = card.IsReadyToReview();

            // Assert
            Assert.True(isReady, "Karta z pudełka 2 powinna być dostępna po 3 dniach.");
        }
    }
}