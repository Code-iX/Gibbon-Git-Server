using System.ComponentModel.DataAnnotations;

namespace Gibbon.Git.Server.Middleware.Attributes;

public class EmailAttribute() : RegularExpressionAttribute(@"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,63}$");