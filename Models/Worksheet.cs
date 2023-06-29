namespace LearnApi.Models;

public class Worksheet
{
    public long Id { get; set; }
    public string Title { get; set; }
    public List<List<Symbol>> Exercises { get; set; }
    public long? ProfileId { get; set; }

    public Worksheet()
    {
    }

    public Worksheet(string _title, List<List<Symbol>> _exercises, long? _profileId)
    {
        foreach (var exercise in _exercises)
        {
            foreach (var symbol in exercise)
            {
                Console.WriteLine("checking.. " + symbol.value);
                if (!isValidLiteral(symbol.value, symbol.type) && !isValidOperator(symbol.value, symbol.type))
                {
                    throw new Exception($"This symbol: {symbol.value} is not allowed or does not match its operand : {symbol.type}!");
                }
            }
        }
        
        Title = _title;
        Exercises = _exercises;
        ProfileId = _profileId;
    }

    private bool isValidOperator(string _value, string _type)
    {
        List<string> allowedOperators = new List<string> { "+", "-", "*", "/" };
        if (allowedOperators.Contains(_value) && _type == "operator")
        {
            return true;
        }       
        return false;
    }

    private bool isValidLiteral(string _value, string _type)
    {
         bool isInteger = int.TryParse(_value, out int result);
         if (isInteger && _type == "literal")
         {
             return true;
         }
         return false;
    }
}