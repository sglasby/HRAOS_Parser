using System.Collections.Generic;
using System.Windows.Forms;

namespace HRAOS_take2
{
    public partial class Form1 : Form
    {
        public static STDOUT stdout;
        public static List<Archetype> arch_list;  // TODO: Object_Registrar keeps something analogous???
        public static List<Obj>       obj_list;

        public Form1()
        {
            InitializeComponent();
            stdout = new STDOUT(richTextBox1); // set up printf() for the richTextBox...
                        
            //create_some_archetypes();  // Test results look good
            //create_some_objs();        // Test results look good

            //parse_sample_file(@"Sample_Script_Text.v1.txt");
            //parse_sample_file(@"Sample_script.round_trip.v1.txt");  // Text emitted from the previous, looks good
            //parse_sample_file(@"round_trip.pass_2.txt");            // Text emitted from the previous, 2nd round trip, looks good

            //parse_sample_file(@"Sample_erroroneous_script.v1.txt");  // Error handling is pretty awful ATM, could be enormously better...

            parse_sample_file(@"Sample_Arch_Obj_script.v2.txt");
            //parse_sample_file(@"Sample_Script.OBJ_round_trip.pass_1.txt");
            //parse_sample_file(@"NonOrderedObjFieldsTest.txt");
        }

        public void create_some_archetypes()
        {
            Archetype aa = new Archetype("first_test_archetype", 0);
            Archetype bb = new Archetype("arch_2", 0);
            Archetype cc = new Archetype("arch_3", 0);
            Archetype dd = new Archetype("arch_4", 0);
            Archetype ee = new Archetype("various_tests", 0);

            aa.add_field("int_2a",     FieldType.INT);
            aa.add_field("string_2a",  FieldType.STRING);
            aa.add_field("decimal_2a", FieldType.DECIMAL);
            aa.add_field("ID_2a",      FieldType.ID);
            //
            stdout.print("Scalars, 2-arg add_field() with the default default value:\n");
            stdout.print(aa.ToString());
            stdout.print("\n");

            bb.add_field("int_field",     FieldType.INT,      42);
            bb.add_field("string_field",  FieldType.STRING,   "abc");
            bb.add_field("decimal_field", FieldType.DECIMAL,  144.99M);
            bb.add_field("ID_field",      FieldType.ID,       0);
            //
            stdout.print("Scalars, 3-arg add_field() with specified default value:\n");
            stdout.print(bb.ToString());
            stdout.print("\n");

            cc.add_field("int_list",     FieldType.LIST_INT);
            cc.add_field("string_list",  FieldType.LIST_STRING);
            cc.add_field("decimal_list", FieldType.LIST_DECIMAL);
            cc.add_field("ID_list",      FieldType.LIST_ID);
            //
            stdout.print("List types, 2-arg add_field() (empty lists):\n");
            stdout.print(cc.ToString());
            stdout.print("\n");

            // Can the syntax of the list-args at the end be improved?  Is it important to do so, given the nature of the typical caller of this?
            // If the caller was a human typing this stuff, might change to multiple methods names, to get rid of the 2nd arg.  But caller is the parser, or editor code.
            dd.add_field("int_list",      FieldType.LIST_INT,     new List<int>     { 1, 2, 3} );
            dd.add_field("string_list",   FieldType.LIST_STRING,  new List<string>  { "abc", "def", "hij" } );
            dd.add_field("decimal_list",  FieldType.LIST_DECIMAL, new List<decimal> { 1.01M, 2.02M, 3.03M } );
            dd.add_field("ID_list",       FieldType.LIST_ID,      new List<int>     { 42, 69, 144 } );

            stdout.print("List types, 3-arg add_field() (non-empty lists):\n"); 
            stdout.print(dd.ToString());
            stdout.print("\n");

            //aa["int_2"] = new ArchetypeField("int_2", FieldType.INT, 123);
            //aa["str_2"] = new ArchetypeField("str_2", FieldType.STRING, "xyz");
            //aa.remove_field("int_field");
            //aa.remove_field("string_field");
            //aa["int_list"].default_value.ilist = new List<int> { 99, 98, 97, 96 };

            //stdout.print("Various tests:\n");
            //stdout.print(ee.ToString());
            //stdout.print("\n");

        }

        public void create_some_objs()
        {
            Archetype aa = new Archetype("first_test_archetype", 7);

            // W00t, it looks like the FieldTempNull approach got rid of the need for these manual add_field() calls
            //aa.add_field("int_field",          FieldType.INT);
            //aa.add_field("string_field",       FieldType.STRING);
            //aa.add_field("decimal_field",      FieldType.DECIMAL);
            //aa.add_field("list_int_field",     FieldType.LIST_INT);
            //aa.add_field("list_string_field",  FieldType.LIST_STRING);
            //aa.add_field("list_decimal_field", FieldType.LIST_DECIMAL);

            Obj obj1 = new Obj(aa, "first_test_obj", 0);

            stdout.print("--- Scalar field types: ---\n");
            int ii = obj1["int_field"].iv;
            obj1["int_field"].iv = 99;
            stdout.print("ii={0},  obj1[].iv={1}\n", ii, obj1["int_field"].iv);  // expect ii=0, obj1[].iv=99

            string ss = obj1["string_field"].sv;
            obj1["string_field"].sv = "def hij";
            stdout.print("ss='{0}', obj1[].sv='{1}'\n", ss, obj1["string_field"].sv);  // expect ss='', obj1[].sv="def hij"

            decimal dd = obj1["decimal_field"].dv;
            obj1["decimal_field"].dv = 123.456M;
            stdout.print("dd={0},  obj1[].dv={1}\n", dd, obj1["decimal_field"].dv);  // expect dd=0, obj1[].dv=123.456

            stdout.print("\n");

            //if (false) {
            //    // Debug output related to the inheritance-of-interface bug found in FieldID and FieldListID:
            //    FieldID argh = new FieldID();
            //    stdout.print("FieldID                               : GetType={0}, type={1}\n", argh.GetType(), argh.type);  // Returns FieldID and ID -- CORRECT

            //    IObjField xzzy = new FieldID();
            //    stdout.print("IObjField, manual cast upon use       : GetType={0}, type={1}\n", ((FieldID) xzzy).GetType(), ((FieldID) xzzy).type);  // Returns FieldID and ID -- CORRECT, have to manually cast upon use??? In the name of all that is static typing, WHY???

            //    IObjField horg = (FieldID) new FieldID();
            //    stdout.print("IObjField, manual cast upon assignment: GetType={0}, type={1}\n", horg.GetType(), horg.type);  // Returns FieldID and INT -- WRONG, why??? (cast did not make the difference)

            //    IObjField slar = new FieldID();
            //    stdout.print("IObjField, no cast                    : GetType={0}, type={1}\n", slar.GetType(), slar.type);  // Returns FieldID and INT -- WRONG, why???
            //    // (The why is due to "implementation of interfaces" not being inherited; 
            //    // thus FieldID and FieldListID needed to explicitly indicate that they implement IObjField

            //    stdout.print("\n");
            //}

            aa.add_field("ID_field_1", FieldType.ID);

            //stdout.print("aa[ID_field_1].type   = {0}\n", aa["ID_field_1"].type);    // Prints ID
            //stdout.print("obj1[ID_field_1].type = {0}\n", obj1["ID_field_1"].type);  // Prints ID (but not if FieldID does not explicitly specify implmentation of IObjField)
            //stdout.print("\n");

            obj1["ID_field_1"].iv = 7;  // 7 is known valid ID
            int ID1 = obj1["ID_field_1"].iv;
            stdout.print("ID={0,3}, obj1[].iv={1}\n", ID1, obj1["ID_field_1"].iv);  // Expect ID=7, obj1[].iv=7
            stdout.print("\n");

            stdout.print("--- List field types: ---\n");

            obj1["list_int_field"    ].ilist = new List<int>     { 1, 2, 3 };
            obj1["list_int_field"    ].ilist.Add(42);
            obj1["list_string_field" ].slist = new List<string>  { "abc", "def", "hij" };
            obj1["list_string_field" ].slist.Add("xyz");
            obj1["list_decimal_field"].dlist = new List<decimal> { 1.1M, 2.2M, 3.3M };
            obj1["list_decimal_field"].dlist.Add(999.999M);
            stdout.print("list_int_field     = {0}\n", obj1["list_int_field"].ToString());
            stdout.print("list_string_field  = {0}\n", obj1["list_string_field" ].ToString() );
            stdout.print("list_decimal_field = {0}\n", obj1["list_decimal_field"].ToString() );
            stdout.print("\n");

            Archetype a1 = new Archetype("foo_arch", 100);
            Obj o1 = new Obj(a1, "foo_o1", 101);
            Archetype a2 = new Archetype("bar_arch", 200);
            Obj o2 = new Obj(a2, "bar_o2", 201);
            Obj o3 = new Obj(a2, "bar_o3", 202);
            Obj o4 = new Obj(a2, "bar_o4", 203);
            obj1["list_ID_field"].ilist = new List<int> { 100, 101, 200, 201 };
            obj1["list_ID_field"].ilist.Add(a1.ID);  // ID == 100, Dupe Archetype ID in list is valid
            obj1["list_ID_field"].ilist.Add(o3.ID);  // ID == 202
            obj1["list_ID_field"].ilist.Add(o1.ID);  // ID == 101, Dupe Obj ID in list is valid
            obj1["list_ID_field"].ilist.Add(o4.ID);  // ID == 203

            stdout.print("list_ID_field      = {0}\n", obj1["list_ID_field"].ToString() );  // Expect [100, 101, 200, 201, 100, 202, 101, 203]
        }

        public void test_set_invalid_ID_values_in_Obj()
        {
            stdout.print("--- ID field types (set invalid ID values): ---\n");
            Archetype bad_arch = new Archetype("arch_to_set_invalid_ID_fields", 0);
            bad_arch.add_field("invalid_ID", FieldType.ID);
            bad_arch.add_field("invalid_LIST_ID", FieldType.LIST_ID);

            Obj bad_obj = new Obj(bad_arch, "obj_to_get_invalid_ID_fields", 0);

            bad_obj["invalid_ID"].iv = 9999;  // Known invalid ID -- should fail (does fail, OK)
            stdout.print("bad_obj[invalid_ID] = {0}\n", bad_obj["invalid_ID"].ToString());

            bad_obj["invalid_LIST_ID"].ilist = new List<int> { 9999, 8888, 7777 };  // Known invalid IDs -- should fail (does fail, OK)
            stdout.print("bad_obj[invalid_LIST_ID] = {0}\n", bad_obj["invalid_LIST_ID"].ToString());

            // The add_field() method is available to control the type which would otherwise be auto-vivified:
            bad_obj.add_field("obj_added_ID_field", FieldType.ID);  // Without this, the field becomes an INT upon auto-vivify
            bad_obj["obj_added_ID_field"].iv = 999;  // 999 is known-invalid ID -- this fails because we called add_field() earlier
            int ID = bad_obj["obj_added_ID_field"].iv;
            stdout.print("bad_obj[obj_added_ID_field].type = {0}\n", bad_obj["obj_added_ID_field"].type);  // prints ID, would print INT if we had not done add_field() earlier
            stdout.print("ID={0,3}, bad_obj[].iv={1}\n", ID, bad_obj["obj_added_ID_field"].iv);  // Expect ID=999, obj1[].iv=999  (if we had gotten this far)
        }

        public void parse_sample_file(string filename)
        {
            arch_list = new List<Archetype>();
            obj_list  = new List<Obj>();

            Script_Parser pp = new Script_Parser(filename);
            pp.parse();

            stdout.print("--- Serializing parsed ARCHETYPEs: ---\n");
            stdout.print("\n");
            foreach (Archetype aa in arch_list)
            {
                stdout.print("{0}", aa.serialize() );
                stdout.print("\n");
            }
            stdout.print("\n");

            stdout.print("--- Serializing parsed OBJs: ---\n");
            stdout.print("\n");
            foreach (Obj oo in obj_list)
            {
                stdout.print("{0}", oo.serialize());
                stdout.print("\n");
            }
            stdout.print("\n");

        }

    } // class Form1

} // namespace
