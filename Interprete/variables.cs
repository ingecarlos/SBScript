using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SBScript.Interprete.expresiones;
namespace SBScript.Interprete
{
    public class variables
    {
        public List<variable> lista = new List<variable>();

        public nodo_expresion add_variable(String tipo, String nombre, String valor) {
            //buscar si existe
            foreach (variable variable in lista) {
                if (nombre.Equals(variable.nombre) && tipo.Equals(variable.tipo)) {
                    return new nodo_expresion("variable \"" + nombre + "\" de tipo " + tipo + " ya definida", "Error Semantico", "", -1, -1);
                }
            }
            //si no existe declararla
            lista.Add(new variable(tipo, nombre, valor));
            return new nodo_expresion("1", "1", "1", 1, 1);
        }

        public variable getVariable(String tipo, String nombre) {
            foreach (variable variable in lista) {
                if (variable.nombre.Equals(nombre)&& variable.tipo.Equals(tipo) ) {
                    return variable;
                }
            }
            return null;
        }

        public variable getVariable(String nombre)
        {
            foreach (variable variable in lista)
            {
                if (variable.nombre.Equals(nombre))
                {
                    return variable;
                }
            }
            return null;
        }

    }
}
