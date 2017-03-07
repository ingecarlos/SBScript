using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Irony.Parsing;
using Irony.Ast;

namespace SBScript
{
    public class Analizador : Grammar
    {
        public Analizador() {

            //Comentarios

            CommentTerminal comentario_bloque = new CommentTerminal("comentario_bloque", "#*", "*#");
            CommentTerminal comentario_linea = new CommentTerminal("comentario_linea", "#", "\n");




            Terminal
                //Palabras reservadas
                TIPO_STRING = new RegexBasedTerminal("String"),
                RETORNO = new RegexBasedTerminal("Retorno"),
                TIPO_NUMBER = new RegexBasedTerminal("Number"),
                TIPO_BOOL = new RegexBasedTerminal("Bool"),
                DEFINE = new RegexBasedTerminal("Define"),
                INCLUYE = new RegexBasedTerminal("Incluye"),
                VOID = new RegexBasedTerminal("Void"),
                PRINCIPAL = new RegexBasedTerminal("Principal"),
                SI = new RegexBasedTerminal("Si"),
                SINO = new RegexBasedTerminal("Sino"),
                SELECCIONA = new RegexBasedTerminal("Selecciona"),
                DEFECTO = new RegexBasedTerminal("Defecto"),
                PARA = new RegexBasedTerminal("Para"),
                HASTA = new RegexBasedTerminal("Hasta"),
                MIENTRAS = new RegexBasedTerminal("Mientras"),
                CONTINUAR = new RegexBasedTerminal("Continuar"),
                DETENER = new RegexBasedTerminal("Detener"),
                //EXPRESIONES REGULARES
                BOOLEANO_TRUE = new RegexBasedTerminal("BOOLEANO_TRUE","True"),
                BOOLEANO_FALSE = new RegexBasedTerminal("BOOLEANO_FALSE","False"),
                ARCHIVO_SBS = new RegexBasedTerminal("archivo_sbs", "[a-zA-Z]([0-9a-zA-Z_])*.sbs"),
                NUMERO = new NumberLiteral("NUMERO"),
                CADENA = new StringLiteral("CADENA", "\""),
                //SIMBOLOS EXPRESIONES
                S_AUMENTO = new RegexBasedTerminal("aumento", "[+][+]"),
                S_DECREMENTO = new RegexBasedTerminal("decremento", "[-][-]"),
                S_MAS = new RegexBasedTerminal("mas", "[+]"),
                S_MENOS = new RegexBasedTerminal("menos", "[-]"),
                S_POR = new RegexBasedTerminal("por", "[*]"),
                S_DIV = new RegexBasedTerminal("div", "[/]"),
                S_PORCENTAJE = new RegexBasedTerminal("porcentaje", "[%]"),
                S_POT = new RegexBasedTerminal("pot", "\\^"),
                //SIMBOLOS RELACIONALES
                S_IGUAL_IGUAL = new RegexBasedTerminal("igual_igual", "=="),
                S_DIF = new RegexBasedTerminal("dif", "!="),
                S_MENOR = new RegexBasedTerminal("menor", "<"),
                S_MAYOR = new RegexBasedTerminal("mayor", ">"),
                S_MENORIGUAL = new RegexBasedTerminal("menorigual", "<="),
                S_MAYORIGUAL = new RegexBasedTerminal("mayorigual", ">="),
                S_SEMEJANTE = new RegexBasedTerminal("semejante", "[~]"),
                //SIMBOLOS LOGICOS
                S_AND = new RegexBasedTerminal("and", "&&"),
                S_OR = new RegexBasedTerminal("or", "\\|\\|"),
                S_XOR = new RegexBasedTerminal("xor", "\\|&"),
                S_NOT = new RegexBasedTerminal("not", "!")
                ;

            IdentifierTerminal ID = new IdentifierTerminal("ID");


            NonTerminal
                estructura = new NonTerminal("estructura"),
                encabezado = new NonTerminal("encabezado"),
                sentencia_encabezado = new NonTerminal("sentencia_encabezado"),
                incluye = new NonTerminal("incluye"),
                incerteza = new NonTerminal("incerteza"),
                ruta = new NonTerminal("ruta"),

                global = new NonTerminal("global"),
                local = new NonTerminal("local"),
                sentencia_global = new NonTerminal("sentencia_global"),
                sentencia_local = new NonTerminal("sentencia_local"),
                retorno_noterminal = new NonTerminal("retorno_noterminal"),

                declaracion = new NonTerminal("declaracion"),
                declarar_funcion = new NonTerminal("declarar_funcion"),
                tipo = new NonTerminal("tipo"),
                lista_id = new NonTerminal("lista_id"),
                asigna = new NonTerminal("asigna"),
                asignacion = new NonTerminal("asignacion"),
                lista_parametros = new NonTerminal("lista_parametros"),
                parametro = new NonTerminal("parametro"),
                llamar_funcion = new NonTerminal("llamar_funcion"),
                lista_expreciones = new NonTerminal("lista_expreciones"),

                call_fun = new NonTerminal("call_funcion"),

                flujo_si = new NonTerminal("flujo_si"),
                cond = new NonTerminal("cond"),
                flujo_sino = new NonTerminal("flujo_sino"),
                flujo_selecciona = new NonTerminal("flujo_selecciona"),
                par_valor = new NonTerminal("par_valor"),
                flujo_defecto = new NonTerminal("flujo_defecto"),
                pares = new NonTerminal("pares"),
                flujo_para = new NonTerminal("flujo_para"),
                paso = new NonTerminal("paso"),
                flujo_hasta = new NonTerminal("flujo_hasta"),
                flujo_mientras = new NonTerminal("flujo_mientras"),

                sentencia_continuar = new NonTerminal("sentencia_continuar"),
                sentencia_detener = new NonTerminal("sentencia_detener"),

                exprecion = new NonTerminal("exprecion"),
                condicion = new NonTerminal("condicion"),
                relacion = new NonTerminal("relacion"),
                op_rel = new NonTerminal("op_rel"),

                valor = new NonTerminal("valor")
                ;
                

            estructura.Rule = encabezado + global;

            encabezado.Rule = MakePlusRule(encabezado,sentencia_encabezado)
                                | Empty
                                ;

            sentencia_encabezado.Rule = incluye
                                        | incerteza
                                        | ruta
                                        ;
            sentencia_encabezado.ErrorRule =SyntaxError;

            global.Rule = MakePlusRule(global, sentencia_global);

            local.Rule = MakePlusRule(local, sentencia_local);

            sentencia_global.Rule = declarar_funcion
                                    | declaracion;

            sentencia_global.ErrorRule = SyntaxError + ToTerm(";");

            sentencia_local.Rule = asignacion
                                   | retorno_noterminal
                                   | declaracion
                                   | call_fun
                                   | flujo_si 
                                   | flujo_selecciona
                                   | flujo_para
                                   | flujo_hasta
                                   | flujo_mientras
                                   | sentencia_continuar
                                   | sentencia_detener
                                    ;

            sentencia_local.ErrorRule = SyntaxError + ToTerm(";");

            call_fun.Rule = llamar_funcion + ToTerm(";");

            sentencia_continuar.Rule = ToTerm("Continuar") + ToTerm(";");

            sentencia_detener.Rule = ToTerm("Detener") + ToTerm(";");

            flujo_hasta.Rule= ToTerm("Hasta") + ToTerm("(") + cond + ToTerm(")") + ToTerm("{") + local + ToTerm("}");

            flujo_mientras.Rule = ToTerm("Mientras") + ToTerm("(") + cond + ToTerm(")") + ToTerm("{") + local + ToTerm("}");

            flujo_para.Rule = ToTerm("Para") + ToTerm("(") + TIPO_NUMBER + ID + ToTerm("=") + exprecion + ToTerm(";")
                               + condicion + ToTerm(";") + paso + ToTerm(")") + ToTerm("{") + local + ToTerm("}");


            paso.Rule = ToTerm("++")
                        |ToTerm("--")
                        ;

            flujo_selecciona.Rule = ToTerm("Selecciona") + ToTerm("(") + exprecion + ToTerm(")") + pares + flujo_defecto;

            pares.Rule = MakePlusRule(pares, par_valor);

            par_valor.Rule = CADENA + ToTerm(":") + ToTerm("{") + local + ToTerm("}")
                            | NUMERO + ToTerm(":") + ToTerm("{") + local + ToTerm("}")
                            ;
            flujo_defecto.Rule = ToTerm("Defecto") + ToTerm(":") + ToTerm("{") + local + ToTerm("}")
                                | Empty
                                ;

            flujo_si.Rule = ToTerm("Si") + ToTerm("(") + cond + ToTerm(")") + ToTerm("{")+ local+ ToTerm("}")+ flujo_sino;

            flujo_sino.Rule = ToTerm("Sino") + ToTerm("{") + local + ToTerm("}")
                            | Empty
                            ;

            cond.Rule = condicion
                        | exprecion;



            retorno_noterminal.Rule = ToTerm("Retorno") + exprecion + ToTerm(";")
                           | ToTerm("Retorno") + ToTerm(";")
                           ; 

            declarar_funcion.Rule = tipo + ID + ToTerm("(") + lista_parametros + ToTerm(")") + ToTerm("{") + local + ToTerm("}")
                                    |PRINCIPAL+ ToTerm("(") + ToTerm(")") + ToTerm("{") + local + ToTerm("}") 
                                    ;

            lista_parametros.Rule = MakePlusRule(lista_parametros, ToTerm(","), parametro)
                                   | Empty
                                   ;


            parametro.Rule = tipo + ID;
            
            declaracion.Rule = tipo + lista_id + asigna + ToTerm(";");

            asignacion.Rule = ID + ToTerm("=") + exprecion + ToTerm(";")
                            | ID + ToTerm("=") + condicion + ToTerm(";")
                            ;

            llamar_funcion.Rule = ID + ToTerm("(") + lista_expreciones + ToTerm(")");

            lista_expreciones.Rule = MakePlusRule(lista_expreciones, ToTerm(","), exprecion)
                                    | Empty;



            asigna.Rule = ToTerm("=") + exprecion
                        | ToTerm("=") + condicion
                        | Empty
                        ;

            lista_id.Rule = MakePlusRule(lista_id, ToTerm(","), ID);

            tipo.Rule = TIPO_NUMBER
                        | ToTerm("String")
                        | TIPO_BOOL
                        | VOID
                        ;
         


            incluye.Rule = INCLUYE + ARCHIVO_SBS;
            incerteza.Rule = DEFINE + NUMERO;
            ruta.Rule= DEFINE + CADENA;


            exprecion.Rule = exprecion + S_MAS + exprecion
                            | exprecion + S_MENOS + exprecion
                            | exprecion + S_POR + exprecion
                            | exprecion + S_DIV + exprecion
                            | exprecion + S_PORCENTAJE + exprecion
                            | exprecion + S_POT + exprecion
                            | S_MENOS + exprecion
                            | ToTerm("(") + exprecion + ToTerm(")")
                            | valor
                ;

            relacion.Rule = exprecion + op_rel + exprecion;

            op_rel.Rule = S_IGUAL_IGUAL
                        | S_DIF
                        | S_MAYOR
                        | S_MENOR
                        | S_MAYORIGUAL
                        | S_MENORIGUAL
                        | S_SEMEJANTE
                        ;

            condicion.Rule = condicion + S_OR + condicion
                            | condicion + S_XOR + condicion
                            | condicion + S_AND + condicion
                            | S_NOT + condicion
                            | ToTerm("(") + condicion + ToTerm(")")
                            | relacion
                            ;


            valor.Rule = NUMERO
                        | CADENA
                        | ToTerm("True")
                        | ToTerm("False")
                        | ID
                        | llamar_funcion
                        ;



            this.Root = estructura;



            //Especificaciones de gramatica
            base.NonGrammarTerminals.Add(comentario_bloque);
            base.NonGrammarTerminals.Add(comentario_linea);

            MarkPunctuation("Incluye", "Define", "(", ")",";",",","{","}","Si","Sino","Selecciona","Defecto",":","Para","Hasta","Mientras");
            MarkTransient(sentencia_encabezado,sentencia_global,tipo,valor,op_rel,paso,cond);

            RegisterOperators(1, Associativity.Left, S_OR);
            RegisterOperators(2, Associativity.Left, S_XOR);
            RegisterOperators(3, Associativity.Left, S_AND);

            RegisterOperators(4,Associativity.Left, S_MAS,S_MENOS);
            RegisterOperators(5,Associativity.Left, S_POR, S_DIV,S_PORCENTAJE);
            RegisterOperators(6, Associativity.Right, S_POT);

            

        }
    }
}
