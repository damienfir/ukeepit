using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SafeBox.Burrow
{
    class WordSequence
    {
        public static string[] Words = {"outside", "special", "two", "open", "airplane", "love", "lake", "death", "talk", "eye", "air", "lemon", "light", "today", "blood", "fly", "salad", "small", "learn", "top", "yes", "key", "blue", "man", "center", "winter", "garden", "leg", "friend", "autumn", "wheel", "word", "son", "chicken", "year", "run", "injured", "difficult", "say", "simply", "source", "empty", "short", "bicycle", "thin", "education", "woman", "bell", "head", "name", "weak", "yellow", "poison", "ruler", "distribute", "white", "sugar", "stone", "zero", "heavy", "uncle", "tomorrow", "aunt", "salt", "no", "boy", "separate", "date", "shelf", "fight", "summit", "cousin", "earth", "mouse", "letter", "cold", "wood", "knowledge", "day", "happy", "laugh", "orange", "fresh", "seven", "long", "tongue", "table", "past", "mix", "sun", "line", "question", "many", "old", "grandmother", "door", "elephant", "sleep", "taxi", "shoes", "hair", "burn", "wind", "four", "slow", "arm", "three", "strong", "finger", "above", "summer", "tooth", "foot", "spring", "valley", "chair", "fast", "wash", "banana", "aluminum", "expert", "easy", "inside", "feather", "clock", "leaf", "knife", "freezing", "rice", "cloud", "jump", "sign", "road", "star", "piano", "mountain", "test", "tiny", "speak", "before", "water", "square", "here", "lion", "right", "remember", "trousers", "hammer", "hot", "decrease", "sick", "round", "gold", "desert", "nose", "fork", "thunderstorm", "house", "tiger", "bridge", "painting", "fingernail", "train", "ball", "scissors", "glass", "pain", "prison", "far", "throw", "hope", "play", "king", "drink", "future", "near", "young", "now", "pencil", "bed", "green", "red", "silence", "wife", "one", "yesterday", "drawing", "horse", "fish", "warm", "night", "big", "meeting", "apple", "increase", "potato", "mouth", "map", "river", "left", "black", "see", "grandfather", "watch", "between", "boat", "allowed", "eat", "thick", "closed", "pen", "husband", "daughter", "birth", "sand", "nine", "egg", "rainfall", "smile", "half", "tired", "listen", "cat", "dog", "bus", "after", "ten", "field", "forbidden", "iron", "girl", "island", "tree", "baby", "flower", "six", "ocean", "knee", "bird", "eight", "below", "bottom", "loud", "answer", "wall", "full", "swim", "moon", "silver", "five", "circle", "bear", "staircase", "soft", "book", "quick"};
        public static Dictionary<string, int> WordIndex = CreateDictionary();

        private static Dictionary<string, int> CreateDictionary() {
            var dictionary = new Dictionary<string, int>();
            for (var i = 0; i < Words.Length; i++) {
                dictionary.Add(Words[i], i);
            }
            return dictionary;
        }

        public static string[] BytesToWords(byte[] bytes) { return BytesToWords(bytes, 0, bytes.Length); }

        public static string[] BytesToWords(byte[] bytes, int offset, int length)
        {
            string[] words = new string[length];
            for (var i = 0; i < length; i++) words[i] = Words[bytes[offset + i]];
            return words;
        }

        public static byte[] WordsToBytes(string[] words)
        {
            byte[] bytes = new byte[words.Length];
            for (var i = 0; i < bytes.Length; i++) {
                var index = 0;
                if (!WordIndex.TryGetValue(words[i], out index)) return null;
                bytes[i] = (byte)index;
            }
            return bytes;
        }

    }
}
