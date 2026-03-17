using Bookstore.SharedKernel.Specifications;
using Shouldly;
using Xunit;

namespace Bookstore.Application.Tests.Specifications;

public sealed class SpecificationEvaluatorTests
{
    [Fact]
    public void Apply_ShouldFilterByCriteria_WhenCriteriaIsSet()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new(1, "Alpha"),
            new(2, "Beta"),
            new(3, "Alpha")
        };
        var spec = new TestCriteriaSpecification(name: "Alpha");

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(e => e.Name == "Alpha");
    }

    [Fact]
    public void Apply_ShouldOrderAscending_WhenOrderByIsSet()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new(3, "C"),
            new(1, "A"),
            new(2, "B")
        };
        var spec = new TestOrderByAscendingSpecification();

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Select(e => e.Id).ShouldBe([1, 2, 3]);
    }

    [Fact]
    public void Apply_ShouldOrderDescending_WhenOrderByDescendingIsSet()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new(1, "A"),
            new(3, "C"),
            new(2, "B")
        };
        var spec = new TestOrderByDescendingSpecification();

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Select(e => e.Id).ShouldBe([3, 2, 1]);
    }

    [Fact]
    public void Apply_ShouldApplyPaging_WhenPagingIsEnabled()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new TestEntity(i, $"Item{i}")).ToList();
        var spec = new TestPaginatedSpecification(page: 2, pageSize: 3);

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Count.ShouldBe(3);
        result.Select(e => e.Id).ShouldBe([4, 5, 6]);
    }

    [Fact]
    public void Apply_ShouldReturnAll_WhenNoSpecificationClausesAreSet()
    {
        // Arrange
        var items = new List<TestEntity>
        {
            new(1, "A"),
            new(2, "B")
        };
        var spec = new TestEmptySpecification();

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Apply_ShouldCombineCriteriaAndPaging()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new TestEntity(i, i % 2 == 0 ? "Even" : "Odd")).ToList();
        var spec = new TestCriteriaWithPagingSpecification(name: "Even", page: 1, pageSize: 2);

        // Act
        var result = SpecificationEvaluator.Apply(items.AsQueryable(), spec).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(e => e.Name == "Even");
    }

    private sealed record TestEntity(int Id, string Name);

    private sealed class TestCriteriaSpecification : Specification<TestEntity>
    {
        public TestCriteriaSpecification(string name)
        {
            ApplyCriteria(e => e.Name == name);
        }
    }

    private sealed class TestOrderByAscendingSpecification : Specification<TestEntity>
    {
        public TestOrderByAscendingSpecification()
        {
            ApplyOrderBy(e => e.Id);
        }
    }

    private sealed class TestOrderByDescendingSpecification : Specification<TestEntity>
    {
        public TestOrderByDescendingSpecification()
        {
            ApplyOrderByDescending(e => e.Id);
        }
    }

    private sealed class TestPaginatedSpecification : Specification<TestEntity>
    {
        public TestPaginatedSpecification(int page, int pageSize)
        {
            ApplyPaging(page, pageSize);
        }
    }

    private sealed class TestEmptySpecification : Specification<TestEntity>;

    private sealed class TestCriteriaWithPagingSpecification : Specification<TestEntity>
    {
        public TestCriteriaWithPagingSpecification(string name, int page, int pageSize)
        {
            ApplyCriteria(e => e.Name == name);
            ApplyPaging(page, pageSize);
        }
    }
}
