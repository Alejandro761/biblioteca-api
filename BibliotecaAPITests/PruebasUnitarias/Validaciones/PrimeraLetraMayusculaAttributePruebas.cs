using BibliotecaAPI.Validaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliotecaAPITests.PruebasUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributePruebas
    {
        [TestMethod]
        [DataRow(" ")]
        [DataRow("     ")]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("Alejandro")]
        public void IsValid_RetornaExitoso_SiValueNoTieneLaPrimeraLetraMinuscula(string value) {
            //Preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());
            //var value = string.Empty;

            //Prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);

            //Verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);
        }
        
        [TestMethod]
        [DataRow("alejandro")]
        public void IsValid_RetornaError_SiValueTieneLaPrimeraLetraMinuscula(string value) {
            //Preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());
            //var value = string.Empty;

            //Prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);

            //Verificacion
            Assert.AreEqual(expected: "La primer letra debe ser mayuscula", actual: resultado!.ErrorMessage);
        }
    }
}
