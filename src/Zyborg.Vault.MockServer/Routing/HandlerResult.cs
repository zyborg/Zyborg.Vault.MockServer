using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.MockServer.Routing
{
    public interface IHandlerResult
    {
        Task EvaluateAsync(HttpContext context);
    }

    public abstract class HandlerResult : IHandlerResult
    {
        public abstract Task EvaluateAsync(HttpContext context);
    }

    public class HandlerResult<TValue> : IHandlerResult
    {
        public HandlerResult(TValue value) =>
                Value = value;

        public HandlerResult(HandlerResult result) =>
                Result = result ?? throw new ArgumentNullException(nameof(result));

        public HandlerResult Result { get; }

        public TValue Value { get; }

        public static implicit operator HandlerResult<TValue>(TValue value) =>
                new HandlerResult<TValue>(value);

        public static implicit operator HandlerResult<TValue>(HandlerResult result) =>
                new HandlerResult<TValue>(result);

        public IHandlerResult ToResult() =>
                Result ?? new Results.ObjectResult(Value);

        Task IHandlerResult.EvaluateAsync(HttpContext context) =>
                ToResult().EvaluateAsync(context);
    }
}