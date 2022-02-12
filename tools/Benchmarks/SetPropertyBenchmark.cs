using System.Reflection;
using BenchmarkDotNet.Attributes;
using Faithlife.Reflection;

namespace Benchmarks;

public class SetPropertyBenchmark
{
	[Benchmark]
	public void Native()
	{
		m_dto.Id = 2L;
	}

	[Benchmark]
	public void RawReflection()
	{
		typeof(BenchmarkDto).GetProperty("Id")!.SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void CachedReflection()
	{
		s_idProperty.SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void SetProperty()
	{
		s_dtoInfo.GetProperty("Id").SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void SetPropertyT()
	{
		s_dtoInfo.GetProperty<long?>("Id").SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void SetPropertyLambda()
	{
		s_dtoInfo.GetProperty(x => x.Id).SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void CachedSetProperty()
	{
		s_dtoProperty.SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void CachedSetPropertyT()
	{
		s_dtoPropertyT.SetValue(m_dto, 2L);
	}

	[Benchmark]
	public void CachedSetPropertyT2()
	{
		s_dtoPropertyT2.SetValue(m_dto, 2L);
	}

	private readonly BenchmarkDto m_dto = new() { Id = 1L, Name = "one" };

	private static readonly PropertyInfo s_idProperty = typeof(BenchmarkDto).GetProperty("Id")!;
	private static readonly DtoInfo<BenchmarkDto> s_dtoInfo = DtoInfo.GetInfo<BenchmarkDto>();
	private static readonly IDtoProperty s_dtoProperty = s_dtoInfo.GetProperty<long?>("Id");
	private static readonly IDtoProperty<BenchmarkDto> s_dtoPropertyT = s_dtoInfo.GetProperty<long?>("Id");
	private static readonly DtoProperty<BenchmarkDto, long?> s_dtoPropertyT2 = s_dtoInfo.GetProperty<long?>("Id");
}
