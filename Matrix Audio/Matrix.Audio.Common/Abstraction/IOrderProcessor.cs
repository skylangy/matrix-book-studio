using Matrix.Audio.Models;

namespace Matrix.Audio.Common.Abstraction;
public interface IOrderProcessor
{
    Task<OrderProcessResult> Process(Order order);
}
