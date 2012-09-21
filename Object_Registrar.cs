using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HRAOS_take2
{
    class Object_Registrar
    {
        public static Object_Registrar All;  // Or maybe Main is a better name?

        // It may be useful to instantiate more than one registrar, primarily 
        // for the purpose of having a collection to loop through all objects 
        // of a given category (all objects, all Objs, all Archetypes, all Items, etc).
        // This depends on what scheme is used to determine what "type" are registered in a particular registry.
        // 
        // If a better means to do this is thought up, I would presumably do that instead.
        // 
        // TODO: 
        // Some representation of the "type" or "category" of objects contained.
        // For now, a singleton registrar holding all registered objects of any C# class, and of any Archetype or category...
        // string category;  // Proposed: object with auto-tag "ARCH-12345" has category "ARCH" and ID 12345

        public int num_objs   { get; private set; }
        public int highest_ID { get; private set; }

        Dictionary<int, IHaximaSerializeable> objects_by_ID;
        Dictionary<IHaximaSerializeable, int> IDs_by_object;
        Dictionary<string, int> IDs_by_tag;

        static Object_Registrar()
        {
            All = new Object_Registrar();
        }

        public Object_Registrar()  // TODO: Add arg for "type" stored in registry...
        {
            num_objs   = 0;
            highest_ID = 0;
            objects_by_ID = new Dictionary<int, IHaximaSerializeable>();
            IDs_by_object = new Dictionary<IHaximaSerializeable, int>();
            IDs_by_tag    = new Dictionary<string, int>();
        } // ObjectRegistrar(archetype, prefix)


        /*****************************************/

        public bool is_valid_or_zero_ID(int the_ID)
        {
            if (the_ID == 0)                   { return true; }
            if (object_for_ID(the_ID) != null) { return true; }
            return false;
        }

        public bool is_valid_ID(int the_ID)
        {
            if (object_for_ID(the_ID) != null) { return true; }
            return false;
        }
        
        /*****************************************/


        public int ID_for_object(IHaximaSerializeable obj)
        {
            // This method is redundant, since registered objects will have an ID() method, 
            // but we define it for the sake of completeness.
            int ID = 0;
            if (obj == null)
            {
                throw new ArgumentException("Got null object");
            }
            bool found = IDs_by_object.TryGetValue(obj, out ID);
            if (!found)
            {
                //Console.WriteLine("ObjectRegistrar.ID_for_obj() obj '{0}' not found\n", obj);
            }
            return ID;
        } // ID_for_object()

        public IHaximaSerializeable object_for_ID(int ID)
        {
            IHaximaSerializeable obj = null;
            bool found = objects_by_ID.TryGetValue(ID, out obj);
            if (!found)
            {
                //Console.WriteLine("ObjectRegistrar.obj_for_ID() ID '{0}' not found\n", ID);
            }
            return obj;
        } // object_for_ID()

        public int ID_for_tag(string tag)
        {
            // If the tag matches either the autotag or the manual tag of a registered object, we return the ID
            int ID = 0;
            bool found = IDs_by_tag.TryGetValue(tag, out ID);
            if (!found)
            {
                //Console.WriteLine("ObjectRegistrar.ID_for_tag() tag '{0}' not found\n", tag);
            }
            return ID;
        } // ID_for_tag()

        public int register_object(IHaximaSerializeable obj, int new_ID)
        {
            // The new_ID arg is either 0 (meaning auto-assign an ID),
            // or non-zero (meaning use the given ID).
            if (obj == null)
            {
                throw new ArgumentException("Got null object");
            }
            int ID;
            if (new_ID == 0)
            {
                // Passing a zero ID means to auto-assign a new one.
                // 
                // This is the case for all new objects created at run-time, 
                // as well as some new objects hand-edited in the script.
                ID = ++highest_ID;
            }
            else
            {
                // Passing a non-zero ID means to use the given ID.
                // 
                // We must check for a collision, and perhaps update the highest_ID 
                // to prevent a future collision on auto-assign.
                // 
                // This is the case for previously-saved objects (which serialized their ID into the script),
                // as well as some new objects hand-edited in the script,
                // or in the case or hand-editing re-ordering of object constructors.

                if (objects_by_ID.ContainsKey(new_ID))
                {
                    // There is a collision with an already-assigned ID.
                    // This can happen if a hand-edited script contains an object constructor call which specifies an already-used ID.
                    // 
                    // TODO: Less-lethal error handling, or a more precise diagnostic (cooperating with the parser)...
                    Error.BadArg("register_obj() Got an already-in-use ID: {0}", new_ID);
                }
                ID = new_ID;
                if (ID >= highest_ID)
                {
                    highest_ID = ID;
                }
            }
            objects_by_ID.Add(ID, obj);
            IDs_by_object.Add(obj, ID);
            // Caller will call register_tag() for .autotag and .tag after this returns...
            num_objs++;
            //Form1.stdout.print("registered 0x{0:X} as ID {1}, now {2} objects registered.\n", obj.GetHashCode(), ID, num_objs);
            return ID;
        } // register_object()

        public void unregister_object(IHaximaSerializeable obj)
        {
            int ID = ID_for_object(obj);
            if (ID == 0)
            {
                throw new ArgumentException("Called for not-previously-registered object");
            }
            objects_by_ID.Remove(ID);
            IDs_by_object.Remove(obj);
            num_objs--;
            //Form1.stdout.print("un-registered 0x{0:X} from ID {1}, now {2} objects registered.\n", obj.GetHashCode(), ID, num_objs);
        } // unregister_object()

        public void register_tag(string tag, int ID)
        {
            if (IDs_by_tag.ContainsKey(tag))
            {
                Error.BadArg("register_tag() called for already-present tag '{0}'.", tag);
            }
            IDs_by_tag[tag] = ID;
        } // register_tag()

        public void unregister_tag(string tag, int ID)
        {
            // Called from some method in an IHaximaSerializeable when a tag is changed / unset.
            // Uses two parameters so that doing an unregister for a tag that does not belong to that object is hard to do accidentally.
            int registered_ID;
            if (!IDs_by_tag.TryGetValue(tag, out registered_ID))
            {
                Error.BadArg("unregister_tag() called for not-registered tag '{0}'.", tag);
            }
            if (ID != registered_ID)
            {
                Error.BadArg("unregister_obj() called for wrong object?  tag='{0}' ID={1}, but found ID={2}", tag, ID, registered_ID);
            }
            IDs_by_tag.Remove(tag);
        } // unregister_tag()


        /*****************************************/

        // TODO: 
        // Consider whether defining a block of C#-type-specific methods such as this block,
        // matching the type-agnostic block of methods above, is a good idea.
        // 
        // If so, a comparable block of methods for at least (Archetype, Obj, Sprite, ...) would be wanted.
        // 
        // ??? Will this actually catch any errors, which would otherwise 
        //     go uncaught, or be caught in a puzzling-rather-than-obvious fashion ???
        // 
        public int ID_for_Archetype(Archetype arch)
        {
            return ID_for_object(arch);
        } // ID_for_Archetype()

        public Archetype Archetype_for_ID(int ID)
        {
            // The main benefit of this method, is to avoid a typecast on the part of the caller.
            object obj = object_for_ID(ID);
            if (obj.GetType() != typeof(Archetype)) {Error.BadArg("Called for non-Archetype");}
            return (Archetype) obj;
        } // Archetype_for_ID()

        //public Archetype Archetype_for_tag(string tag)
        //{
        //    object obj = object_for_tag(tag);
        //    if (obj.GetType() != typeof(Archetype)) { Error.BadArg("Called for non-Archetype"); }
        //    return (Archetype) obj;
        //} // Archetype_for_tag()

        public void register_Archetype(Archetype arch, int new_ID)
        {
            register_object(arch, new_ID);
        } // register_Archetype()

        public void unregister_Archetype(Archetype arch)
        {
            unregister_object(arch);
        } // unregister_Archetype()


        /*****************************************/


    } // class Object_Registrar



    public interface IHaximaSerializeable
    {
        int    ID      { get; set; }  // A positive integer, 0 is special non-valid value
        string autotag { get; }       // Of the form "prefix-ID", such as "SPR-12345"
        string tag     { get; set; }  // Either null, or of a form which does not collide with any autotag
        void   register(int ID);      // this.ID = Object_Registrar.All.register_obj(this)
        void   unregister();          // Object_Registrar.All.unregister_object(this)
    } // interface IHaximaSerializeable

} // namespace
