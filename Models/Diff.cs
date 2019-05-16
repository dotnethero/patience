namespace Patience.Models
{
    public class Diff
    {
        public Operation Operation { get; private set; }
        public string Text { get; private set; }

        public Diff(Operation operation, string text) 
        {
            Operation = operation;
            Text = text;
        }
        
        public void Prepend(string prefix)
        {
            Text = prefix + Text;
        }

        public void Append(string suffix)
        {
            Text = Text + suffix;
        }

        public void Update(string newText)
        {
            Text = newText;
        }
        
        public override string ToString()
        {
            return $"{Operation}: {Text}";
        }
    }
}