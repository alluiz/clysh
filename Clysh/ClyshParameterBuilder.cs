using System.Text.RegularExpressions;
using Clysh.Helper;

namespace Clysh;

public class ClyshParameterBuilder: ClyshBuilder<ClyshParameter>
{
    public ClyshParameterBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }
    
    public ClyshParameterBuilder Regex(Regex regex)
    {
        Result.Regex = regex;
        return this;
    }
    
    public ClyshParameterBuilder Required(bool required)
    {
        Result.Required = required;
        return this;
    }
    
    public ClyshParameterBuilder MinLength(int minLength)
    {
        Result.MinLength = minLength;
        return this;
    }
    
    public ClyshParameterBuilder MaxLength(int maxLength)
    {
        Result.MaxLength = maxLength;
        return this;
    }
}