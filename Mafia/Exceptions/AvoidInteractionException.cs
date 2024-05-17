namespace Mafia.Exceptions;

public class UnknownUsersException : Exception
{
    public UnknownUsersException() :base("All users should be determined for each player. That means all user roles are known")
    {        
    }
}

public class AvoidInteractionException : Exception
{
}

public class AlreadyRunException: Exception
{
}
