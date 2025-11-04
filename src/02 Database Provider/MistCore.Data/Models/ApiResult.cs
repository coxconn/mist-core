using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MistCore.Data
{
    [Serializable]
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ApiResult<T>
    {
        [DataMember(Name = "Code")]
        public virtual int Code { get; set; }

        [DataMember(Name = "Message")]
        public virtual string Message { get; set; }

        [DataMember(Name = "Data")]
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
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ApiResult
    {
        [DataMember(Name = "Code")]
        public virtual int Code { get; set; }

        [DataMember(Name = "Message")]
        public virtual string Message { get; set; }

        [DataMember(Name = "Data")]
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
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ResultInfo<T>
    {
        [DataMember(Name = "Detail")]
        public virtual T Detail { get; set; }

        [DataMember(Name = "List")]
        public virtual List<T> List { get; set; }

        [DataMember(Name = "Page")]
        public PageInfo Page { get; set; }

        public ResultInfo()
        {
        }

        public ResultInfo(T detail)
        {
            this.Detail = detail;
        }

        public ResultInfo(List<T> list, PageInfo page)
        {
            this.List = list;
            this.Page = page;
        }
    }

    [Serializable]
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ResultInfo
    {
        [DataMember(Name = "Detail")]
        public virtual object Detail { get; set; }

        [DataMember(Name = "List")]
        public virtual object List { get; set; }

        [DataMember(Name = "Page")]
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
    [DataContract(Namespace = "http://tempuri.org/")]
    public class ResultInfo<T, E>
    {
        [DataMember(Name = "Detail")]
        public virtual E Detail { get; set; }

        [DataMember(Name = "List")]
        public virtual List<T> List { get; set; }

        [DataMember(Name = "Page")]
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
