using System.Reflection;
using BenchmarkDotNet.Attributes;
using Faithlife.Reflection;

namespace Benchmarks;

public class GetPropertyBenchmark
{
	[Benchmark]
	public void Native()
	{
		m_name = m_dto.Name;
	}

	[Benchmark]
	public void RawReflection()
	{
		m_name = (string?) typeof(BenchmarkDto).GetProperty("Name")!.GetValue(m_dto);
	}

	[Benchmark]
	public void CachedReflection()
	{
		m_name = (string?) s_nameProperty.GetValue(m_dto);
	}

	[Benchmark]
	public void GetProperty()
	{
		m_name = (string?) s_dtoInfo.GetProperty("Name").GetValue(m_dto);
	}

	[Benchmark]
	public void GetPropertyT()
	{
		m_name = s_dtoInfo.GetProperty<string>("Name").GetValue(m_dto);
	}

	[Benchmark]
	public void GetPropertyLambda()
	{
		m_name = s_dtoInfo.GetProperty(x => x.Name).GetValue(m_dto);
	}

	[Benchmark]
	public void CachedGetProperty()
	{
		m_name = (string?) s_dtoProperty.GetValue(m_dto);
	}

	[Benchmark]
	public void CachedGetPropertyT()
	{
		m_name = (string?) s_dtoPropertyT.GetValue(m_dto);
	}

	[Benchmark]
	public void CachedGetPropertyT2()
	{
		m_name = s_dtoPropertyT2.GetValue(m_dto);
	}

	private readonly BenchmarkDto m_dto = new() { Id = 1L, Name = "one" };
	private string? m_name;

	private static readonly PropertyInfo s_nameProperty = typeof(BenchmarkDto).GetProperty("Name")!;
	private static readonly DtoInfo<BenchmarkDto> s_dtoInfo = DtoInfo.GetInfo<BenchmarkDto>();
	private static readonly IDtoProperty s_dtoProperty = s_dtoInfo.GetProperty<string>("Name");
	private static readonly IDtoProperty<BenchmarkDto> s_dtoPropertyT = s_dtoInfo.GetProperty<string>("Name");
	private static readonly DtoProperty<BenchmarkDto, string> s_dtoPropertyT2 = s_dtoInfo.GetProperty<string>("Name");
}
