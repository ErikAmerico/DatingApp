// using System;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

// Read your secret key from config (and guard its strength).
// Encode that key & build a SymmetricSecurityKey.
// Pack your user info into claims.
// Create signing creds with HMAC-SHA512.
// Describe your token (claims + expiry + signing).
// Generate the token object.
// Serialize it to a string.
// When you hand that string to clients, they include it in their Authorization: Bearer <token> header.
//// Your ASP .NET Core middleware will then:
// Verify the signature with the same secret key
// Check the expiry
// Extract the claims so your controllers know who is calling

//declaring a class TokenService that implements ITokenService.
public class TokenService(IConfiguration config) : ITokenService
//(IConfiguration config) is C#’s “primary constructor” syntax—ASP .NET Core will
//“inject” your app’s configuration object here so you can read appsettings.json.
{
    public string CreateToken(AppUser user)
    {
        //expect an entry in appsettings.json (or environment) called "TokenKey"
        //this is your private secret used to sign tokens.
        var tokenKey =
            config["TokenKey"] ?? throw new Exception("Cannot access tokenKey from appsettings");

        //Symmetric algorithms like HMAC-SHA512 demand a strong secret (64+ characters).
        if (tokenKey.Length < 64)
            throw new Exception("Your tokenKey needs to be longer");

        //Encoding.UTF8.GetBytes(...) converts your string key into a byte array.
        //SymmetricSecurityKey wraps those bytes in a form the JWT library understands.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        //A Claim is a piece of data you bake into the token’s payload.
        //Here you’re saying “this token’s subject is user.UserName” under the standard NameIdentifier claim type.
        //You can add more (roles, emails, anything).
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.UserName) };

        //A SigningCredentials object pairs your key + a crypto algorithm (HmacSha512Signature).
        //This tells the JWT library: “Sign the token with HMAC-SHA512 using this key.”
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        //Subject wraps your list of claims into a JWT “subject” (who this token represents)
        //Expires sets the token’s lifetime—here 7 days from now.
        //SigningCredentials is how you’ll prove the token came from your server and wasn’t tampered with.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
        };

        //Actually building the JWT
        ////JwtSecurityTokenHandler is the workhorse from System.IdentityModel.Tokens.Jwt.
        var tokenHandler = new JwtSecurityTokenHandler();
        //CreateToken(...) consumes your descriptor and
        //// spits out a JwtSecurityToken object (headers + payload + signature in-memory).
        var token = tokenHandler.CreateToken(tokenDescriptor);

        //WriteToken(...) turns that object into the standard 3-part header.payload.signature string you can return in an HTTP response.
        return tokenHandler.WriteToken(token);
    }
}
