﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Data.Update
{
    public class InsertDefaultData : IUpdateScript
    {
        public string Command
        {
            get
            {
                Guid roleId = Guid.NewGuid();
                Guid UserId = Guid.NewGuid();
                return @"

                    INSERT INTO [Role] ([Id], [Name], [Description]) VALUES ('" + roleId.ToString() + @"','Administrator','System administrator');
                    INSERT INTO [User] ([Id], [Name], [Surname], [Username], [Password], [Email]) VALUES ('"+ UserId.ToString() + @"','admin', '', 'admin', '0CC52C6751CC92916C138D8D714F003486BF8516933815DFC11D6C3E36894BFA044F97651E1F3EEBA26CDA928FB32DE0869F6ACFB787D5A33DACBA76D34473A3', '');
                    INSERT INTO [UserRole_InRole] ([User_Id], [Role_Id]) VALUES ('"+ UserId.ToString() + "','" + roleId.ToString() + @"');

                    ";
            }
        }

        public string Precondition
        {
            get { return @"SELECT Count(*) = 0 FROM [User]"; }
        }
    }
}