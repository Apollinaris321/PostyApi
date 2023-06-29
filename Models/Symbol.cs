using Microsoft.AspNetCore.Mvc.Localization;

namespace LearnApi.Models;

public class Symbol : ValueObject
{
    
    public string type { get; set; }
    
    public string value { get; set; }

    private bool isValidValue(string _value)
    {
         List<string> allowedOperators = new List<string> { "+", "-", "*", "/" };
         if (allowedOperators.Contains(_value))
         {
             return true;
         }
         bool isInteger = int.TryParse(_value, out int result);
         if (isInteger)
         {
             return true;
         }
         return false;
    }

    public Symbol() {}

    public Symbol(string _value)
    {
        List<string> allowedOperators = new List<string> { "+", "-", "*", "/" };
        if (allowedOperators.Contains(_value))
        {
            value = _value;
            type = "operator";
            //type = SymbolType.Operator;
        }
        bool isInteger = int.TryParse(_value, out int result);
        if (isInteger)
        {
            value = _value;
            type = "literal";
            //type = SymbolType.Literal;
        }
        throw new Exception("_value is neither Operator nor Literal!");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return value;
        yield return type;
    }
}

public enum SymbolType
{
    Operator,
    Literal
}