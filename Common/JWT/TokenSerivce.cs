using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWT
{
    public class TokenSerivce : ITokenService
    {
        public string BuildToken(IEnumerable<Claim> claims, JWTOptions options)
        {
            TimeSpan ExpriyDuration = TimeSpan.FromSeconds(options.ExpireSeconds);
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key));
            var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(options.Issuer, options.Audience, claims,
                expires:DateTime.Now.Add(ExpriyDuration),signingCredentials:credential);
             string token= new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            return token;
        }
    }
}
