using Microsoft.AspNetCore.Http;
using System;

namespace QuizApp.Tests.Controllers.Fakes
{
    internal class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext
        {
            get => new DefaultHttpContext();
            set => throw new NotImplementedException();
        }
    }
}