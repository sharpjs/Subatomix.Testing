// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

#if !NET8_0_OR_GREATER
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using static System.Reflection.BindingFlags;
#endif

namespace Subatomix.Testing;

/// <summary>
///   Base class for tests of exception types.
/// </summary>
/// <typeparam name="T">
///   The exception type to test.
/// </typeparam>
public abstract class ExceptionTests<T>
    where T : Exception
{
    // As told by Dennis Ritchie:
    // https://www.bell-labs.com/usr/dmr/www/odd.html
    private const string ArcaneMessage = "values of β will give rise to dom!";

#if !NETCOREAPP3_1
    // Squelch warnings about missing XML comments: tests will not have XML comments
    #pragma warning disable CS1591
#endif

    [Test]
    public virtual void Construct_Default()
    {
        var exception = Create();

        exception.Message       .ShouldNotBeNullOrWhiteSpace();
        exception.InnerException.ShouldBeNull();
    }

    [Test]
    public virtual void Construct_Message()
    {
        var exception = Create(ArcaneMessage);

        exception.Message       .ShouldBeSameAs(ArcaneMessage);
        exception.InnerException.ShouldBeNull();
    }

    [Test]
    public virtual void Construct_Message_Null()
    {
        var exception = Create(null as string);

        exception.Message       .ShouldNotBeNullOrWhiteSpace();
        exception.InnerException.ShouldBeNull();
    }

    [Test]
    public virtual void Construct_MessageAndInnerException()
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(ArcaneMessage, innerException);

        exception.Message       .ShouldBeSameAs(ArcaneMessage);
        exception.InnerException.ShouldBeSameAs(innerException);
    }

    [Test]
    public virtual void Construct_MessageAndInnerException_NullMessage()
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(null as string, innerException);

        exception.Message       .ShouldNotBeNullOrWhiteSpace();
        exception.InnerException.ShouldBeSameAs(innerException);
    }

    [Test]
    public virtual void Construct_MessageAndInnerException_NullInnerException()
    {
        var exception = Create(ArcaneMessage, null as Exception);

        exception.Message       .ShouldBeSameAs(ArcaneMessage);
        exception.InnerException.ShouldBeNull();
    }

#if !NET8_0_OR_GREATER
    [Test]
    public void SerializableAttribute()
    {
        typeof(T).IsSerializable.ShouldBeTrue();
    }

    [Test]
    public void DeserializationConstructor()
    {
        var constructor = typeof(T).GetConstructor(
            Instance | Public | NonPublic | ExactBinding,
            null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            null
        );

        constructor.ShouldNotBeNull(string.Format(
            "exception type {0} must provide a deserialization constructor; " +
            "see notes in the test for further information",
            typeof(T).FullName
        ));

        constructor!.IsFamily.ShouldBeTrue(string.Format(
            "exception type {0} deserialization constructor must be protected; " +
            "see notes in the test for further information",
            typeof(T).FullName
        ));
    }

    [Test]
    public virtual void SerializeThenDeserialize() // tests protected serialization constructor
    {
        var innerException = new InvalidProgramException();
        var exception      = Create(ArcaneMessage, innerException);

        var deserialized   = Roundtrip(exception);

        deserialized               .ShouldNotBeNull();
        deserialized.Message       .ShouldBe(ArcaneMessage);
        deserialized.InnerException.ShouldBeOfType<InvalidProgramException>();
    }

    // Squelch Visual Studio's incorrect hint to remove the next suppression
    #pragma warning disable IDE0079

    // Allow obsolete BinaryFormatter to test legacy deserialization constructor
    // https://docs.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib0011
    #pragma warning disable SYSLIB0011

    private static T Roundtrip(T obj)
    {
        using var memory = new MemoryStream();

        // Serialize
        var formatter = new BinaryFormatter();
        formatter.Serialize(memory, obj);

        // Rewind
        memory.Position = 0;

        // Deserialize
        return (T) formatter.Deserialize(memory);
    }
#endif

    private static T Create(params object?[] args)
    {
        // T is Exception or some type derived from it, not Nullable<_>.
        // Thus Activator.CreateInstance is guaranteed to not return null.
        return (T) Activator.CreateInstance(typeof(T), args)!;
    }
}
