using System;
using Clysh.Core;
using Clysh.Core.Builder;
using Clysh.Helper;
using NUnit.Framework;

namespace Clysh.Tests;

public class ClyshParameterTests
{
    private ClyshParameterBuilder _parameterBuilder = new ();

    [Test]
    public void ShouldThrownErrorWithTooLongId()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_LONG_PARAMETER_ID_NAME";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdLength);
    }
    
    [Test]
    public void ShouldThrownErrorWithEmptyId()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdLength);
    }
    
    [Test]
    public void ShouldThrownErrorWithIdWithBlankSpaces()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "MY TEST";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateIdPattern);
    }
    
    [Test]
    public void ShouldThrownErrorWithoutRange()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateParameterRange);
    }
    
    [Test]
    public void ShouldThrownErrorWithMinGreatherThanMaxRange()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var exception = Assert.Throws<ArgumentException>(() => _parameterBuilder
            .Id(id)
            .Range(10, 1)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateParameterMaxLength);
    }
    
    [Test]
    public void ShouldThrownErrorWithInvalidRange()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Range(-1, 1)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateParameterRange);
    }
    
    [Test]
    public void ShouldThrownErrorWithRangeTooLong()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Range(1, 101)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateParameterRange);
    }
    
    [Test]
    public void ShouldThrownErrorWithNegativeOrder()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var exception = Assert.Throws<EntityException>(() => _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(-1)
            .Build());
        
        ExtendedAssert.MatchMessage(exception?.Message!, ClyshMessages.ErrorOnValidateParameterOrder);
    }
    
    [Test]
    public void ShouldPassWithValidValuesAsRequired()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .MarkAsRequired()
            .Build();
        
        Assert.AreEqual(id, parameter.Id);
        Assert.AreEqual(1, parameter.MinLength);
        Assert.AreEqual(100, parameter.MaxLength);
        Assert.AreEqual(1, parameter.Order);
        Assert.IsTrue(parameter.Required);
    }
    
    [Test]
    public void ShouldPassWithValidValuesAsOptional()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .Build();
        
        Assert.AreEqual(id, parameter.Id);
        Assert.AreEqual(1, parameter.MinLength);
        Assert.AreEqual(100, parameter.MaxLength);
        Assert.AreEqual(1, parameter.Order);
        Assert.IsFalse(parameter.Required);
    }
    
    [Test]
    public void ShouldPassWithValidInput()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .Build();

        var myData = "my input text";
        parameter.Data = myData;

        Assert.AreEqual(id, parameter.Id);
        Assert.AreEqual(1, parameter.MinLength);
        Assert.AreEqual(100, parameter.MaxLength);
        Assert.AreEqual(1, parameter.Order);
        Assert.AreEqual(myData, parameter.Data);
        Assert.AreEqual(parameter.Data, parameter.ToString());
        Assert.IsFalse(parameter.Required);
    }
    
    [Test]
    public void ShouldThrownErrorWithEmptyInput()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .Build();

        var myData = string.Empty;
        
        var exception = Assert.Throws<ArgumentException>(() => parameter.Data = myData);
        
        ExtendedAssert.MatchMessage(exception?.Message!, $"Parameter {parameter.Id} must be not null or empty and between {parameter.MinLength} and {parameter.MaxLength} chars.");
    }
    
    [Test]
    public void ShouldThrownErrorWithTooLongInput()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .Build();

        var myData = "5gAdnvhzsnZpKkROmmGea0PNChSaFPxruiDJPsIsyWXyMLkpWRuCEcK8CbQVpNjqrkDAS1VriTZanRsvdC5Mjlqc1A50q02ZtRTbh";
        
        var exception = Assert.Throws<ArgumentException>(() => parameter.Data = myData);
        
        ExtendedAssert.MatchMessage(exception?.Message!, $"Parameter {parameter.Id} must be not null or empty and between {parameter.MinLength} and {parameter.MaxLength} chars.");
    }
    
    [Test]
    public void ShouldThrownErrorWithInvalidInput()
    {
        _parameterBuilder = new ClyshParameterBuilder();

        const string id = "TEST_ID";

        var parameter =  _parameterBuilder
            .Id(id)
            .Range(1, 100)
            .Order(1)
            .Pattern("^\\w+$")
            .Build();

        var myData = "$@!2131231";
        
        var exception = Assert.Throws<ArgumentException>(() => parameter.Data = myData);
        
        ExtendedAssert.MatchMessage(exception?.Message!, "Parameter {0} must match the follow regex pattern: {1}.");
    }
}