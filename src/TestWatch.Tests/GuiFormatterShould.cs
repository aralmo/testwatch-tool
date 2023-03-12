namespace TestWatch.Tests;
using FluentAssertions;

public class GuiFormatterShould
{
    [Theory]
    [InlineData("SomeMethod", "some method")]
    [InlineData("someMethod", "some method")]
    [InlineData("some_Method", "some method")]
    public void PrettyPrintMethods(string method, string expected)
    => GUI
        .ToPrettyTestName(method)
        .Should()
        .Be(expected);

    [Theory]
    [InlineData("SomeTestClass", "SomeTestClass")]
    [InlineData("SomeTestClassShould", "SomeTestClass should")]
    [InlineData("SomeTestClass_Adapter", "SomeTestClass Adapter")]
    public void PrettyPrintClasses(string classname, string expected)
    => GUI
        .ToPrettyClassName(classname)
        .Should()
        .Be(expected);


}