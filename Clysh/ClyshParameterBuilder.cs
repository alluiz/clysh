using System.Text.RegularExpressions;
using ProjectHelper;

namespace Clysh;

public class ClyshParameterBuilder: Builder<ClyshParameter>
{
    public ClyshParameterBuilder Id(string id)
    {
        this.Result.Id = id;
        return this;
    }
    
    public ClyshParameterBuilder Regex(Regex regex)
    {
        this.Result.Regex = regex;
        return this;
    }
    
    public ClyshParameterBuilder Required(bool required)
    {
        this.Result.Required = required;
        return this;
    }
    
    public ClyshParameterBuilder MinLength(int minLength)
    {
        this.Result.MinLength = minLength;
        return this;
    }
    
    public ClyshParameterBuilder MaxLength(int maxLength)
    {
        this.Result.MaxLength = maxLength;
        return this;
    }
}