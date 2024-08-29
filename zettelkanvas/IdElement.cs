using System.Text.RegularExpressions;

namespace zettelkanvas
{
    internal struct IdElement : IComparable<IdElement>
    {
        public int Number { get; private set; }
        public string Branch { get; private set; }

        public IdElement(string IdElement)
        {
            MatchCollection parts = Regexes.IdElementPart().Matches(IdElement);
            Number = int.Parse(parts[0].Value);
            Branch = "";
            if (parts.Count > 1)
                Branch = parts[1].Value;
        }

        public int CompareTo(IdElement other)
        {
            if (Number < other.Number) return -1;
            if (Number > other.Number) { return 1; }

            return String.Compare(Branch, other.Branch, StringComparison.InvariantCulture);
        }
        public override string ToString()
        {
            return $"{Number}:\"{Branch}\"";
        }
    }
}
