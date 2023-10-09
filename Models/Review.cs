using Humanizer.Localisation.TimeToClockNotation;

namespace MyRestAPI.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public int Score { get; set; }
        public string Text { get; set; }
        public int GameID { get; set; }
    }
}
