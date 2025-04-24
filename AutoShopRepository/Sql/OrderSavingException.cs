using System;

namespace MMN.Repository.Sql
{
    [Serializable]
    internal class OrderSavingException : Exception
    {
        public OrderSavingException()
        {
        }

        public OrderSavingException(string message) : base(message)
        {
        }

        public OrderSavingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}