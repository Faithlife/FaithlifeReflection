using System.Reflection;
using BenchmarkDotNet.Attributes;
using Faithlife.Reflection;

namespace Benchmarks;

public class CreateNewBenchmark
{
	[Benchmark]
	public void Native()
	{
		m_dto = new BenchmarkDto { Id = 1L, Name = "one" };
	}

	[Benchmark]
	public void RawReflection()
	{
		m_dto = Activator.CreateInstance<BenchmarkDto>();
		typeof(BenchmarkDto).GetProperty("Id")!.SetValue(m_dto, 1L);
		typeof(BenchmarkDto).GetProperty("Name")!.SetValue(m_dto, "one");
	}

	[Benchmark]
	public void CachedReflection()
	{
		m_dto = Activator.CreateInstance<BenchmarkDto>();
		s_idProperty.SetValue(m_dto, 1L);
		s_nameProperty.SetValue(m_dto, "one");
	}

	[Benchmark]
	public void DtoInfoCreateNew()
	{
		m_dto = DtoInfo.GetInfo<BenchmarkDto>().CreateNew(("Id", 1L), ("Name", "one"));
	}

	[Benchmark]
	public void CachedDtoInfoCreateNew()
	{
		m_dto = s_dtoInfo.CreateNew(("Id", 1L), ("Name", "one"));
	}

	private BenchmarkDto? m_dto;

	private static readonly DtoInfo<BenchmarkDto> s_dtoInfo = DtoInfo.GetInfo<BenchmarkDto>();
	private static readonly PropertyInfo s_idProperty = typeof(BenchmarkDto).GetProperty("Id")!;
	private static readonly PropertyInfo s_nameProperty = typeof(BenchmarkDto).GetProperty("Name")!;
}
