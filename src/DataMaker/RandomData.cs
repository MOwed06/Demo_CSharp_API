namespace DataMaker
{
    public static class RandomData
    {
        private readonly static Random _randomSource;

        private readonly static List<string> _firstNames;
        private readonly static List<string> _familyNames;
        private readonly static List<string> _sentences;

        static RandomData()
        {
            var randomSeed = Guid.NewGuid().GetHashCode();
            _randomSource = new Random(randomSeed);

            _firstNames = ReadListFromSupportFileText("FirstNames.txt");
            _familyNames = ReadListFromSupportFileText("FamilyNames.txt");
            _sentences = ReadListFromSupportFileText("Poetry_RFrost.txt");
        }

        /// <summary>
        /// Randomly generate probablility event
        /// </summary>
        /// <param name="probability">likelyhood in range 0.0 - 1.0</param>
        /// <returns></returns>
        public static bool Chance(double probability)
        {
            return _randomSource.NextDouble() >= probability;
        }

        public static int GenerateInt(int minValue, int maxValue)
        {
            return _randomSource.Next(minValue, maxValue);
        }

        public static double GenerateDouble(double minValue, double maxValue)
        {
            var range = maxValue - minValue;
            return minValue + range * _randomSource.NextDouble();
        }

        public static decimal GenerateDecimal(decimal minValue, decimal maxValue, int roundDigits = 2)
        {
            var range = maxValue - minValue;
            var randomValue = minValue + range * (decimal)_randomSource.NextDouble();
            return Math.Round(randomValue, roundDigits);
        }

        public static T SelectFromList<T>(List<T> collection)
        {
            var selectedIndex = GenerateInt(0, collection.Count);
            return collection[selectedIndex];
        }

        public static string GenerateFirstName()
        {
            return SelectFromList<string>(_firstNames);
        }

        public static string GenerateFamilyName()
        {
            return SelectFromList<string>(_familyNames);
        }

        public static string GenerateSentence()
        {
            return SelectFromList<string>(_sentences);
        }

        public static string GenerateFullName()
        {
            var firstName = GenerateFirstName();
            var familyName = GenerateFamilyName();

            return $"{firstName} {familyName}";
        }

        private static List<string> ReadListFromSupportFileText(string fileName)
        {
            var fullName = Path.Combine(Directory.GetCurrentDirectory(), "SupportFiles", fileName);
            var fileData = File.ReadAllText(fullName);

            var dataItems = fileData.Split("\n");

            return dataItems
                .Select(x => x.Replace("\r", string.Empty))
                .Select(x => x.Trim())
                .ToList();
        }
    }
}
