using System.Reflection;
using BenchmarkDotNet.Attributes;
using Faithlife.Reflection;

namespace Benchmarks;

public class CreateEmptyBenchmark
{
	[Benchmark]
	public void Native()
	{
		m_dto = new BenchmarkDto();
	}

	[Benchmark]
	public void ActivatorCreateInstance()
	{
		m_dto = (BenchmarkDto) Activator.CreateInstance(typeof(BenchmarkDto))!;
	}

	[Benchmark]
	public void ActivatorCreateInstanceGeneric()
	{
		m_dto = Activator.CreateInstance<BenchmarkDto>();
	}

	[Benchmark]
	public void Constructor()
	{
		m_dto = (BenchmarkDto) typeof(BenchmarkDto).GetConstructor(Array.Empty<Type>())!.Invoke(null);
	}

	[Benchmark]
	public void CachedConstructor()
	{
		m_dto = (BenchmarkDto) s_constructor.Invoke(null);
	}

	[Benchmark]
	public void DtoInfoCreateNew()
	{
		m_dto = DtoInfo.GetInfo<BenchmarkDto>().CreateNew();
	}

	[Benchmark]
	public void CachedDtoInfoCreateNew()
	{
		m_dto = s_dtoInfo.CreateNew();
	}

#pragma warning disable IDE0052 // Remove unread private members
	private BenchmarkDto? m_dto;
#pragma warning restore IDE0052 // Remove unread private members

	private static readonly DtoInfo<BenchmarkDto> s_dtoInfo = DtoInfo.GetInfo<BenchmarkDto>();
	private static readonly ConstructorInfo s_constructor = typeof(BenchmarkDto).GetConstructor(Array.Empty<Type>())!;
}
