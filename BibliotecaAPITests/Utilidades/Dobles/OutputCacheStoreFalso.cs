using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OutputCaching;

namespace BibliotecaAPITests.Utilidades.Dobles
{
    public class OutputCacheStoreFalso : IOutputCacheStore
    {
        public ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
        {
            //sino deseamos implementar el metodo entonces retornamos simplemente que la tarea ha sido completada
            return ValueTask.CompletedTask;            
        }

        public ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
