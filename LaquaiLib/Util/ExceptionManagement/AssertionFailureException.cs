﻿using System.Diagnostics;

namespace LaquaiLib.Util.ExceptionManagement;

/// <summary>
/// The exception that is thrown when an assertion fails.
/// </summary>
public class AssertionFailureException<T> : Exception
{
    private const string _defaultMessage = "Assertion failed.";
    private static string MessageFromCaller
    {
        get
        {
            var method = new StackFrame(1).GetMethod();
            return method is not null ? $"Assertion failed in {method.Name}." : _defaultMessage;
        }
    }

    /// <inheritdoc cref="Exception.Message"/>
    public new string Message { get; }
    /// <inheritdoc cref="Exception.InnerException"/>
    public new Exception? InnerException { get; }

    /// <summary>
    /// The value that caused an assertion to fail.
    /// </summary>
    public T? Value { get; }

    public AssertionFailureException(T? value) : base(MessageFromCaller)
    {
        Value = value;
    }
    public AssertionFailureException(T? value, string message) : this(value)
    {
        Message = string.IsNullOrWhiteSpace(message) ? MessageFromCaller : message;
    }
    public AssertionFailureException(T? value, string message, Exception? innerException) : this(value, message)
    {
        if (innerException is not null)
        {
            InnerException = innerException;
        }
    }
}
