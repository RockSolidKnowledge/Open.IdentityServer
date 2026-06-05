using System;
using System.Collections.Generic;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;

namespace IdentityServer.IntegrationTests.Endpoints.Discovery;

public class FakeIdentityServerKeyStore: IIdentityServerKeyStore
{
    public static readonly DateTime FakeNow = new(2026, 02, 01, 12, 00, 00, DateTimeKind.Utc);
    
    public IEnumerable<IdentityServerKeyMaterial> GetKeys()
    {
        return
        [
            new IdentityServerKeyMaterial { Id = "FakeRSA_RS256", Version = 1, Use = "signing", DataProtected = false, Algorithm = "RS256", Data = FakeRS256KeyData },
            new IdentityServerKeyMaterial { Id = "FakeRSA_PS256_NONSIGN", Version = 1, Use = "other", DataProtected = false, Algorithm = "PS256", Data = FakePS256KeyData },
            new IdentityServerKeyMaterial { Id = "FakeRSA_ES256", Version = 1, Use = "signing", DataProtected = false, Algorithm = "ES256", Data = FakeES256KeyData },
            new IdentityServerKeyMaterial { Id = "FakeRSA_ES384", Version = 1, Use = "signing", DataProtected = false, Algorithm = "ES384", Data = FakeES384KeyData },
            new IdentityServerKeyMaterial { Id = "FakeRSA_ES521", Version = 1, Use = "signing", DataProtected = false, Algorithm = "ES521", Data = FakeES521KeyData },
            new IdentityServerKeyMaterial { Id = "FakeRSA_PS512_Expired", Version = 1, Use = "signing", DataProtected = false, Algorithm = "PS512", Data = FakePS512KeyData },
        ];
    }
    
    private const string FakeRS256KeyData = """
        {
            "Parameters": {
                "D": "BVenqEynwvBFkVej9c4LA/bokTRPgM3sFbtDFjV3Nb8Myn8abpsCs63CRs+A+dY0T7I5FbeiQCRr0k8E38RdbT6FPTNl8Y5Mw3EK63p8yNow4jmj0xllthlcsRG9Bqtkl5YkutNJ68IrscFONAiLAfq3llBwveRg/fTRdi6UcoVPgpesi4RHTHez0DqxfMkDIRJ1vkU0xPIxsUop2SLMxB6Q9bIJnjsgsws1JmiG3iO/bhRF0Bkbz1WWzrRVPtpGipy3apmhbFmy2hs5kAgiw1tdeurPEndG1WjoAv6HmLtKKz6tgl1lbVLX6vgbSJQv70pCXEOWTIsSfvr3CvMZIQ==",
                "DP": "nnzIbpM38+KGDYfLf/mT31n4Bfn7oQsUqWW4dtiFhs9XWHwqbtIgc+UEAe+o++TQRDakUChbR5EdyPP9Hbv/GHCaQnDYTtMpzY16DFmANFlaHlp8kZI7L9PUGDKqtqfFIldKhDZnQM7wZsUybSwX/Tzpd/89tCQbl4eN+keAJgc=",
                "DQ": "iuz0mV3dUP7qB9gOfrSxnwf7WEZB669lr5U1lVI+TgLFRqGv66KqFNgGQjWUiM9sfyTnPQixKF0nnxjl4SMjaSsQw1mC9fabE69agaHvda/MTVL/WSYFgHGNtJ23rq21VE9TXTEAW682IBh2o9iSNWjKCOgZLko8qqA6H20t08E=",
                "Exponent": "AQAB",
                "InverseQ": "Rgz8DKhPbMDzBZJKnqqmNuqP5FEQVy9ixlHx1gjM1QzdXSOBxQM9hJ7xEVVeU72HQaGfONFfs+ywJRTTrGOYYFS0arQC7xSSKnQj84FGm/5M+geme5Libt2Tp2mNxcbUrxJBTHxoLAXvGfjlu7pL5xmYC/GJIOXiN8KoAP8O8i0=",
                "Modulus": "lhB1UxNfZTjo7y41Oj6YVOqk7Lr4kd4MEWkmEgS6OME+N7DPMlHQM0EUH5NswPKl/6gD8YW6vaMPtynhCO6X/tLnAfQXu0R7vPme8/5YsVhHcCzYYUZUUjCe2hCm0oeSdV+oVB9Q7N0RDbvI64upmKXs6WsmWQjR5gxVsWgtU4hnkBXgRMWSQZPTvAaGthGtT18e0xZPiRI3mi7++JonkgVpvln8lHubP4ov6OKOOmxW7yLaLFRyYJIKJpXl9G26QRV8VpwWDEV6klds2n9YNAuQJFCAD1qoFgk4p+JZm3yAFRHpZlQHL83E4LF2ub7bZwy/pSo948Ie0/S21uYiTw==",
                "P": "0OsVcWCQisrgPtmD9Lf3xCKen88SIAgFVkP9FAURlLTj5nUtvBCorP+KjYxa3qsl6Hu/N7OlRY0w+/JLF/w8L3k62t1qxAu5Ad2M1/kvdXm2dVoj+ONUDL6ElzFua377Ybzzx01ngloAwrUp0W8OvRInhOsjp5aV2Tj5BaVbTq8=",
                "Q": "t+H3/N6FXbJRKqUL79gLxB210Ym8kbmrwsyVKiL9h6mnLqlO2mz03FiyPZZVjI7u66BHpilKZ8oQVtzrcXi/vD243qnixmavx6LX5O5K17hJ25O9A9KebeosBTq0YcQwxrowhTVVglxPCg3UPZWA3QNLti4Gq/BhZDGeHWzsTmE="
            },
            "Id": "FakeRSA_RS256",
            "Algorithm": "RS256",
            "HasX509Certificate": false,
            "Created": "2026-01-01T12:00:00"
        }
        """;

    private const string FakePS256KeyData = """
        {
            "Parameters": {
                "D": "ApSWmZpvo/CFfmUQgk9AQPxyurPuqSMX9P50TrZgXsFQuG7TQwjvNDf7+iWnhz1H/iMvbwYyH4dvyK7qNWORERqPWcbwEk03MCWuHtTTNfRMHXFWbEGOrcGymjMZYgcTyt5dooyzB25hcy/VGNfBRDQQLLti0YoMdyw00uaHII+T1bwderfjq9NvbRW5wkRw8ep49iyBQ4NvKaFi2qUe0V+ASQpRhAHm425b4F/SqCl9HLK47eE2DC5dy05ayIFxW9GBpAtaP/7bdIpZHd6i1mBZnOEoScufJM2htuUemGMer2t+K9w3wdfkFL/3ab77FieW9hakHHxS/7NYzJ2aEw==",
                "DP": "uUOTbCrFcqNo4QOx0FpSyJQMqJAiUGMwd3DTGOJZwnlDlmY6e2ZhFcNf6WrSVKuzkSpnCog3ZSemhZEWqYUg9IVH2o/pMV6iYyfMpT1r+U8Tel9N5phZvbO/dt43ydPEtI726Wwd0xxuexZVNhrgiHIQQC7v81VtFAyoOrTpDzU=",
                "DQ": "WwfD1cKGuPDiFaFHfdc5l9oi1/UVO/kfetYBj76FEzU59vgMv1s/iwvy84JUzlCukzQzaf1j68t+xFSMnVDMFFShSVdazpX80jtKwht75sN5R7W1l0epWnC2mBMcSXPmXYTGFA1ct1OxVBM6UehK/9CE+HcIqO6otfRVDOTCiBM=",
                "Exponent": "AQAB",
                "InverseQ": "nSZqgg6aUlBnrHpIFNtlbWQEnG/+ut9U8TpRfEihY5+oKa9RCd/xMlC6LJmMJQbktR6pIqgFIhrzueZNaKT+/5OaGPV8pfjH9saAtE4s1o2JNRNuOSyLjEbd0rNmPvR5S35+leyFT2Fj7Z4d9l5UmDxRab7B43qVtJWSF0kvsCk=",
                "Modulus": "vAy/WZg5J2pVCMjGoegT4VeRTtdsu888LMCw0zBjYsZgbhfhyR2H6iJi7ikR/TaFzFr4kqsRtpoJUHI77FnbPcBr0EIc/X3DL01rfjvZR5E6T5uIGNE1+gPqrYEXSxViVi9XSG3C1YCMusOlcNPvtIsjcIRplvo4/wYtHxmoAbAGgPTuVMN0kZ2IDEKuDmfBIpVdngxGoseOV7kjOI9tH5v4sTU2WbGeSj8GWZNrEAW1AlzlzbxByppnRID37+CATRRN+Si6iU3irdcZlUWkuf45hn32kjRv25J+OPIgZ1bwd+0SfnyagIbzhdFzevVdgojtQgIbqhSmMmI+6bH8WQ==",
                "P": "9m5HzjGJCv0Of6ekvXu6zmTuSQmAqZ8XXSXO0IHJGC0sUaWTaj0wX8sSwtvjMQc+iA8BuDB6HB1yzbu08Zs75WNWHYgz9xqSA1DgsfVizuo7wol73memFKqeB7LYSyE8SLgza5SWV+ema77dwvWh45o/dqiohh9n6hpwH5NyZy8=",
                "Q": "w1oc93pQ/UoAvfJ6zK24BKiVGI1o5uFF0cGvqGsBAxQ4d7jg65aJPpKhbhFJrmGqi6mld9rDYLCYXOiLb+VjXrBV9MoO9EvGXLsLK0x+D7zN8J4b7DoucO6/pwX2kNaYoLTUKCpSisVillcmbXMx0cYMZd3s3s6dSCRfZ9oa8vc="
            },
            "Id": "FakeRSA_PS256_NONSIGN",
            "Algorithm": "PS256",
            "HasX509Certificate": false,
            "Created": "2026-01-01T12:00:00"
        }
        """;

    private const string FakeES256KeyData = """
        {
            "D": "6l0Qd9ZoV5gFj7mrKuzDJvLuaCOAoWiuSuWhJMTFuts=",
            "Q": {
                "X": "2HObQ7JtVdCHb/VBVx+P7YQ71zUHvIZQy62j7zKL2tw=",
                "Y": "Qu+kwCML126rHKqmc3JhsL59sgZrv8B1IXwTwyBbr3U="
            },
            "Id": "FakeEC_ES256",
            "Algorithm": "ES256",
            "HasX509Certificate": false,
            "Created": "2026-01-01T12:00:00"
        }
        """;

    private const string FakeES384KeyData = """
        {
            "D": "FTG9qoWhAOdRtzovZJxq+4ZerL3u1Ji7zF8QNRRcZjpMLT1pLTwsq8ipwhXUjNTE",
            "Q": {
                "X": "IiwiFowDMwIjDlt4FiOq4WTlKFe2MIlzFFCM2A/C2Z1rab9A4a0PZmbmfvGz0pzX",
                "Y": "cLgNQcBdO3Vrknl0qmlftAnHyVtw1UcbP65hHC7y3e4+6g9qBMX4BPjwG4DCd6X3"
            },
            "Id": "FakeEC_ES384",
            "Algorithm": "ES384",
            "HasX509Certificate": false,
            "Created": "2026-01-01T12:00:00"
        }
        """;

    private const string FakeES521KeyData = """
        {
            "D": "AJ7z4XAnvCP2Ea+Xm2Q8ZW7/10caVvPk+uzVw//4WhNSEUN0uNmvPq6/bUswvlBLDfLMgoH9HaI9l+I/De7OKJnj",
            "Q": {
                "X": "AJR23zqtji/WXqM+WKcr9IUBA2nErWkXQoahE8P3MejJfNX4ja5bR3fLx23OYF5vU/bvAmMZuhcl16o8+0+GuGpi",
                "Y": "AIiRICSC2ywpL4tr+7CaWp4nkaIzIJNA2Lt0ip2eFj4grn/qBhBdyy2v6wgnDXqOVELh33gNfoqI9FKxqN+sYUOQ"
            },
            "Id": "FakeEC_ES521",
            "Algorithm": "ES521",
            "HasX509Certificate": false,
            "Created": "2026-01-01T12:00:00"
        }
        """;
    
    private const string FakePS512KeyData = """
        {
            "Parameters": {
                "D": "Bj8nuAP4kWK6Bh5u7JM1SlkENF4MmaCDhptjtoG3mo+Blh7EfRb3IGCNP4E81xmWTBAmnNdb6/H9e0YSublgXVHnXcsEwLCiIucWCa9YmwuUeBw4cMzNIHVsWss2wv7XpY18SVLzZs9PW99zgZwREquQFbYcjVGfEQqEYNGe891Lz+AKTo5Qpsyx3laZLTOUL+VBb5bJGJxl2HIeFWDbrn/bqltBRUcjayYiX563IwtHBwLBWBAEbrBBurGPaIk9PjC4WVAvVdvWYZQgksfePsV7/1zWhi6K5VElAn0ohrpmImHWh6ob+nIV6oUJG/gXpNAmB2ZbMDopeRn1QRZKAQ==",
                "DP": "QUvY4w84l8MoqhAcxoXA7H6DaPZRpDnoNuTPfMVJCjnMxBLrX8k2Yx8vPk2KhFXSklG9K0PrLWsp8EO/dHVplbUXqhluYbeAJ6UZXNXW1cmjw6AXiYn+SwibP7KY6HhElxTVqhMqm+66e6Coei11otsweRMWT3dSmPBMlYoxc/M=",
                "DQ": "2HtvEUst8qBP7yl1n2cMoEBNo6X6Hv8On2oFBvAtsUgzfHO9BKRdrrwo7fGH6t6ZGoGYB+uE9tbenRpr58ZEHHE65bKwZ6z/Us6ogKq/QOsfroQCU509U0x5aXn3DfYwzNGXDDlw7RESh89L9YWf7j9UsMeITPpw1nINf06jIAE=",
                "Exponent": "AQAB",
                "InverseQ": "PUaNIRe81tN+RmxtA4OrfKupweBWPfGrKXosT9IbIlP3aqEGsR0+rLtWzs+5fw0bMyFSLcVUmEQAfso/RirlskqtO0A5Zg2qfQjSVZkTCvLrPYBnu94JwNhA6K/MawkoRcqsM682FVOlJvSqkhbCZuG/6AcF+Lu3To4oDjsm5y4=",
                "Modulus": "zYRdc59usklvJRIgQhK+6jEp0P8YohIj3vYEWyv3S/dHqa8dSGloyHDJhG6RAdqnR8S2nhGTcdokqRE4hSdWMaQSd5FKgC2ZpKe0BM3qt4q7lzid8kezkUqkhqCV39ITEiMxMkeMDK+SZqiH2OB5J6n+G4b5PnroshElcNXRaWultIm+vnAWkPXJyhc0mvD+uAlFd9TMbTU5BZE7NZ0tn8Y0Ny2MAPLWW0UVmkGeSwS5rCROiaJTS3P50BJD5eAJrq4OAilVjciBWWt68+6uwxmUGZye9foqAc98P3jLM7ccLrMKklyejTBNIcisyNEY+3+WM1NkA3mMy0qVS0cbXw==",
                "P": "7khqnAiMQG17a5daoEmL2zcB6rpnJ5t7cVqJd12UUCbfVzhP+pdxvjG5vt4Dpb5/lN29GlC+sJeL9WsmBSwtCENb1No3uUQ8QbQYCFA4gBInck5q2RBRuPeqP4RHJWglR+9rDSs+hcGvNaDPBL70BCyUQKcrlaBSy4NDwBCFZb8=",
                "Q": "3MxE5TRElLXPnY0miYxqGc3zpjKayu/c7yRaM/kR+16+fRi1AWhscVBpusA27xHdS5SZiTGEeLdqiKQ+OVC/oNmXS2aXDaGOyb6014jsil43sBQVQekeiTfkKslPE03DlOr6w6KqeuiKTF+yQBHvH3yybDlXy3wqpiGLq7az8mE="
            },
            "Id": "FakeRSA_PS512_Expired",
            "Algorithm": "PS512",
            "HasX509Certificate": false,
            "Created": "2025-10-01T12:00:00"
        }
        """;
}