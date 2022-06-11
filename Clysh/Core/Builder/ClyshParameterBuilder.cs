using System.Text.RegularExpressions;

namespace Clysh.Core.Builder;

public class ClyshParameterBuilder: ClyshBuilder<ClyshParameter>
{
    public ClyshParameterBuilder Id(string id)
    {
        Result.Id = id;
        return this;
    }
    
    public ClyshParameterBuilder Pattern(string? pattern)
    {
        if (pattern != null)
            Result.Regex = new Regex(pattern);
        
        return this;
    }
    
    public ClyshParameterBuilder Required(bool required)
    {
        Result.Required = required;   
        return this;
    }
    
    public ClyshParameterBuilder Range(int minLength, int maxLength)
    {
        if (minLength < 1)
            throw new ArgumentException($"Invalid min length. The values must be between 1 and 1000.", nameof(minLength));
        
        if (maxLength > 1000)
            throw new ArgumentException($"Invalid max length. The values must be between 1 and 1000.", nameof(maxLength));
        
        Result.MinLength = minLength;
        Result.MaxLength = maxLength;
        
        if (Result.MinLength > Result.MaxLength)
            throw new ArgumentException($"Invalid max length. The max length must be greater than min length.", nameof(Result.MaxLength));
        
        return this;
    }
}