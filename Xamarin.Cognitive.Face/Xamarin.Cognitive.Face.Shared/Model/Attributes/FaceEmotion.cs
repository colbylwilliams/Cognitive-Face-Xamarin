namespace Xamarin.Cognitive.Face.Model
{
    public class FaceEmotion : Attribute
    {
        public float Anger { get; set; }

        public float Contempt { get; set; }

        public float Disgust { get; set; }

        public float Fear { get; set; }

        public float Happiness { get; set; }

        public float Neutral { get; set; }

        public float Sadness { get; set; }

        public float Surprise { get; set; }

        public float MostEmotionValue { get; set; }

        public string MostEmotion { get; set; }

        public override string ToString ()
        {
            return $"Emotion: {MostEmotion} ({MostEmotionValue})";
        }
    }
}