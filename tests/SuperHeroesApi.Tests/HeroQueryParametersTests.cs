using SuperHeroesApi.Application.DTOs;

namespace SuperHeroesApi.Tests
{
  public class HeroQueryParametersTests
  {
    [Fact]
    public void Page_ShouldDefaultToOne()
    {
      var query = new HeroQueryParameters();

      Assert.Equal(1, query.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Page_ShouldBeNormalizedToOne_WhenLessThanOne(int invalidPage)
    {
      var query = new HeroQueryParameters { Page = invalidPage };

      Assert.Equal(1, query.Page);
    }

    [Fact]
    public void PageSize_ShouldDefaultToTen()
    {
      var query = new HeroQueryParameters();

      Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void PageSize_ShouldBeCappedAtMaximum()
    {
      var query = new HeroQueryParameters { PageSize = 1000 };

      Assert.Equal(50, query.PageSize);
    }

    [Fact]
    public void PageSize_ShouldBeNormalizedToOne_WhenLessThanOne()
    {
      var query = new HeroQueryParameters { PageSize = 0 };

      Assert.Equal(1, query.PageSize);
    }

    [Fact]
    public void SortBy_ShouldDefaultToName()
    {
      var query = new HeroQueryParameters();

      Assert.Equal(HeroSortBy.Name, query.SortBy);
    }
  }
}
