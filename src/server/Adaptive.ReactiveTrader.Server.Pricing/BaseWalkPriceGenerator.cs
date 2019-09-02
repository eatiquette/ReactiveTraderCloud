using Adaptive.ReactiveTrader.Contract;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace Adaptive.ReactiveTrader.Server.Pricing
{
  public abstract class BaseWalkPriceGenerator : IDisposable
  {
    protected static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());
    protected readonly Subject<SpotPriceDto> _priceChanges = new Subject<SpotPriceDto>();
    protected readonly object _lock = new object();
    protected readonly int _precision;

    protected decimal _initial;
    protected decimal _previousMid;

    public BaseWalkPriceGenerator(string symbol, decimal initial, int precision)
    {
      Symbol = symbol;
      _initial = _previousMid = initial;
      _precision = precision;
      EffectiveDate = new DateTime(2019, 1, 1);
      SourceName = PriceSource.HardCodedSourceName;
      //PriceChanges = _priceChanges.Replay(1).RefCount();
    }

    public string Symbol { get; }
    public DateTime EffectiveDate { get; private set; }
    public string SourceName { get; private set; }
    public decimal SampleRate => _previousMid;

    public void Dispose()
    {
      _priceChanges.Dispose();
    }

    public void UpdateInitialValue(decimal newValue, DateTime effectiveDate, string sourceName)
    {
      lock (_lock)
      {
        _initial = _previousMid = newValue;
        EffectiveDate = effectiveDate;
        SourceName = sourceName;
      }
      UpdateWalkPrice();
    }

    public abstract void UpdateWalkPrice();

    public IObservable<SpotPriceDto> PriceChanges => _priceChanges;

    public override string ToString() => $"{Symbol}|{EffectiveDate}|{_initial}|{SourceName}";
    

  }
}