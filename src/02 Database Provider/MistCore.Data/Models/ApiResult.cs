using System;
using System.Collections.Generic;
using System.Text;

namespace MistCore.Data
{

    [Serializable]
    public class ApiResult<T>
    {
        public virtual int Code { get; set; }
        public virtual string Message { get; set; }
        public virtual T Data { get; set; }

        public ApiResult()
        {
        }

        public ApiResult(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public ApiResult(int code, string message, T data)
        {
            this.Code = code;
            this.Message = message;
            this.Data = data;
        }
    }

    [Serializable]
    public class ApiResult
    {
        public virtual int Code { get; set; }
        public virtual string Message { get; set; }
        public virtual object Data { get; set; }

        public ApiResult()
        {
        }

        public ApiResult(int code, string message)
        {
            this.Code = code;
            this.Message = message;
        }

        public ApiResult(int code, string message, object data)
        {
            this.Code = code;
            this.Message = message;
            this.Data = data;
        }
    }

    [Serializable]
    public class ResultInfo<T>
    {
        public virtual T detail { get; set; }
        public virtual List<T> List { get; set; }
        public PageInfo Page { get; set; }

        public ResultInfo()
        {
        }

        public ResultInfo(T detail)
        {
            this.detail = detail;
        }

        public ResultInfo(List<T> list, PageInfo page)
        {
            this.List = list;
            this.Page = page;
        }
    }

    [Serializable]
    public class ResultInfo
    {
        public virtual object Detail { get; set; }
        public virtual object List { get; set; }
        public PageInfo Page { get; set; }

        public ResultInfo()
        {
        }

        public ResultInfo(object detail)
        {
            this.Detail = detail;
        }

        public ResultInfo(object list, PageInfo page)
        {
            this.List = list;
            this.Page = page;
        }

        public ResultInfo(object list, PageInfo page, object detail)
        {
            this.List = list;
            this.Page = page;
            this.Detail = detail;
        }
    }

    [Serializable]
    public class ResultInfo<T, E>
    {
        public virtual E Detail { get; set; }
        public virtual List<T> List { get; set; }
        public PageInfo Page { get; set; }

        public ResultInfo()
        {
        }

        public ResultInfo(List<T> list, PageInfo page, E detail)
        {
            this.List = list;
            this.Page = page;
            this.Detail = detail;
        }
    }

}
