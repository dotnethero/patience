namespace Patience.Models
{
    public class Diff
    {
        public Operation Operation { get; set; }
        public string Text { get; set; }

        public Diff(Operation operation, string text) 
        {
            this.Operation = operation;
            this.Text = text;
        }

        protected bool Equals(Diff other)
        {
            return Operation == other.Operation && string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Diff) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Operation * 397) ^ (Text != null ? Text.GetHashCode() : 0);
            }
        }
        
        public override string ToString()
        {
            return $"{Operation}: {Text}";
        }
    }
}