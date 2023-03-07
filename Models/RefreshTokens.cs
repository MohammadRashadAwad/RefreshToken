using Microsoft.EntityFrameworkCore;

namespace RefreshToken.Models
{
    [Owned]
    public class RefreshTokens
    {
        public string Token { get; set; }

        public DateTime ExpiresOn { get; set; } // 6:30

        public bool IsExpired => DateTime.UtcNow >= ExpiresOn;  // 6:31  
        // true == expired 
       // false == not expired

        public DateTime CreatedOn { get; set; }

        public DateTime ? RevokedOn { get; set; }
        // null => not revoked 
        // Date => revoked 

        public bool IsActive => RevokedOn == null &&  !IsExpired;
         
        // true ==> RevokedOn is null  && IsExpired is false (true &&  !false )

        // false ==> RevokedOn have Date value  && IsExpired is True (false && !true)




    }
}
