using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GithubDownloader;
using Xunit;

namespace GithubDownloaderTests
{
    public class LinkHeaderParserTests
    {
       [Fact]
        void EmptyValueReturnsEmptyResult()
        { 
            GetExpectedResult("", string.Empty);
        }

        [Fact]
        void NullValueReturnsEmptyResult()
        {
            GetExpectedResult(null, string.Empty);
        }

        [Fact]
        void OneInvalidValueReturnsEmptryResult()
        {
            GetExpectedResult("InvalidValue", string.Empty);
        }

        [Fact]
        void MultipleInvalidValuesReturnsEmptyResult()
        {
            GetExpectedResult("InvalidValue, InvalidValue", string.Empty);
        }

        [Fact]
        void SingleValidValueReturnsExpectedResult()
        {
            GetExpectedResult(
                "<https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2>; rel=\"next\"", 
                "https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2");
        }

        [Fact]
        void ComplexValudValueReturnsExpectedResult()
        {
            GetExpectedResult(
                "< https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2>; rel=\"next\"," +
                " < https://api.github.com/search/code?q=addClass+user%3Amozilla&page=34>; rel=\"last\"",
                "https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2"
                );
        }

        [Fact]
        void ValidValueWithNoNextReturnsEmptyResult()
        {
            GetExpectedResult(
                "<https://api.github.com/search/code?q=addClass+user%3Amozilla&page=2>; rel=\"last\"",
                "");
        }

        void GetExpectedResult(string value, string expected)
        {
            var parser = new LinkHeaderParser();
            var result = parser.GetNextPageFromHeader(value);
            Assert.Equal(expected, result);
        }
    }
}
