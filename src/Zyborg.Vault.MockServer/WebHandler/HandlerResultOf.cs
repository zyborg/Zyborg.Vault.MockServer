using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
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
                Result ?? Results.Object(Value);

        Task IHandlerResult.EvaluateAsync(HttpContext context) =>
                ToResult().EvaluateAsync(context);
    }
}