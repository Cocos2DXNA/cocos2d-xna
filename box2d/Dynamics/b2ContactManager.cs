using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2D.Collision;
using Box2D.Dynamics.Contacts;

namespace Box2D.Dynamics
{
    public class b2ContactManager
    {
        private b2BroadPhase m_broadPhase;
        private b2Contact m_contactList;
        private int m_contactCount;
        private b2ContactFilter m_contactFilter;
        private b2ContactListener m_contactListener;

        public b2ContactManager()
        {
            m_contactList = null;
            m_contactCount = 0;
            m_contactFilter = b2ContactFilter.b2_defaultFilter;
            m_contactListener = b2ContactListener.b2_defaultListener;
			m_broadPhase = new b2BroadPhase();

        }

        public b2Contact ContactList
        {
            get { return (m_contactList); }
            set { m_contactList = value; }
        }

        public int ContactCount
        {
            get { return (m_contactCount); }
            set { m_contactCount = value; }
        }

        public b2BroadPhase BroadPhase { 
            get { return (m_broadPhase); } 
            set { m_broadPhase = value; } 
        }

        public b2ContactListener ContactListener
        {
            set { m_contactListener = value; }
            get { return (m_contactListener); }
        }

        public b2ContactFilter ContactFilter
        {
            set { m_contactFilter = value; }
            get { return (m_contactFilter); }
        }

        public void Destroy(b2Contact c)
        {
            b2Fixture fixtureA = c.FixtureA;
            b2Fixture fixtureB = c.FixtureB;
            b2Body bodyA = fixtureA.Body;
            b2Body bodyB = fixtureB.Body;

            if (m_contactListener != null && c.IsTouching())
            {
                m_contactListener.EndContact(c);
            }

            // Remove from the world.
            if (c.Prev != null)
            {
                c.Prev.Next = c.Next;
            }

            if (c.Next != null)
            {
                c.Next.Prev = c.Prev;
            }

            if (c == m_contactList)
            {
                m_contactList = c.Next;
            }

            // Remove from body 1
            if (c.NodeA.Prev != null)
            {
                c.NodeA.Prev.Next = c.NodeA.Next;
            }

            if (c.NodeA.Next != null)
            {
                c.NodeA.Next.Prev = c.NodeA.Prev;
            }

            if (c.NodeA == bodyA.ContactList)
            {
                bodyA.ContactList = c.NodeA.Next;
            }

            // Remove from body 2
            if (c.NodeB.Prev != null)
            {
                c.NodeB.Prev.Next = c.NodeB.Next;
            }

            if (c.NodeB.Next != null)
            {
                c.NodeB.Next.Prev = c.NodeB.Prev;
            }

            if (c.NodeB == bodyB.ContactList)
            {
                bodyB.ContactList = c.NodeB.Next;
            }

            c.Free();

            // Call the factory.
            --m_contactCount;
        }

        // This is the top level collision call for the time step. Here
        // all the narrow phase collision is processed for the world
        // contact list.
        public void Collide()
        {
            // Update awake contacts.
            b2Contact c = m_contactList;
            while (c != null)
            {
                b2Fixture fixtureA = c.FixtureA;
                b2Fixture fixtureB = c.FixtureB;
                int indexA = c.m_indexA;
                int indexB = c.m_indexB;
                b2Body bodyA = fixtureA.Body;
                b2Body bodyB = fixtureB.Body;

                // Is this contact flagged for filtering?
                if ((c.Flags & b2ContactFlags.e_filterFlag) != 0)
                {
                    // Should these bodies collide?
                    if (bodyB.ShouldCollide(bodyA) == false)
                    {
                        b2Contact cNuke = c;
                        c = cNuke.Next;
                        Destroy(cNuke);
                        continue;
                    }

                    // Check user filtering.
                    if (m_contactFilter != null && m_contactFilter.ShouldCollide(fixtureA, fixtureB) == false)
                    {
                        b2Contact cNuke = c;
                        c = cNuke.Next;
                        Destroy(cNuke);
                        continue;
                    }

                    // Clear the filtering flag.
                    c.Flags &= ~b2ContactFlags.e_filterFlag;
                }

                bool activeA = (bodyA.BodyFlags & b2BodyFlags.e_awakeFlag) != 0 && bodyA.BodyType != b2BodyType.b2_staticBody;
                bool activeB = (bodyB.BodyFlags & b2BodyFlags.e_awakeFlag) != 0 && bodyB.BodyType != b2BodyType.b2_staticBody;

                // At least one body must be awake and it must be dynamic or kinematic.
                if (activeA == false && activeB == false)
                {
                    c = c.Next;
                    continue;
                }

                int proxyIdA = fixtureA.m_proxies[indexA].proxyId;
                int proxyIdB = fixtureB.m_proxies[indexB].proxyId;
                bool overlap = m_broadPhase.TestOverlap(proxyIdA, proxyIdB);

                // Here we destroy contacts that cease to overlap in the broad-phase.
                if (overlap == false)
                {
                    b2Contact cNuke = c;
                    c = cNuke.Next;
                    Destroy(cNuke);
                    continue;
                }

                // The contact persists.
                c.Update(m_contactListener);
                c = c.Next;
            }
        }

        public void FindNewContacts()
        {

            m_broadPhase.UpdatePairs(this);
        }

        public void AddPair(ref b2FixtureProxy proxyA, ref b2FixtureProxy proxyB)
        {
            b2Fixture fixtureA = proxyA.fixture;
            b2Fixture fixtureB = proxyB.fixture;

            int indexA = proxyA.childIndex;
            int indexB = proxyB.childIndex;

            b2Body bodyA = fixtureA.Body;
            b2Body bodyB = fixtureB.Body;

            // Are the fixtures on the same body?
            if (bodyA == bodyB)
            {
                return;
            }

            // TODO_ERIN use a hash table to remove a potential bottleneck when both
            // bodies have a lot of contacts.
            // Does a contact already exist?
            b2ContactEdge edge = bodyB.ContactList;
            while (edge != null)
            {
                if (edge.Other == bodyA)
                {
                    b2Fixture fA = edge.Contact.FixtureA;
                    b2Fixture fB = edge.Contact.FixtureB;
                    int iA = edge.Contact.m_indexA;
                    int iB = edge.Contact.m_indexB;

                    if (fA == fixtureA && fB == fixtureB && iA == indexA && iB == indexB)
                    {
                        // A contact already exists.
                        return;
                    }

                    if (fA == fixtureB && fB == fixtureA && iA == indexB && iB == indexA)
                    {
                        // A contact already exists.
                        return;
                    }
                }

                edge = edge.Next;
            }



            // Does a joint override collision? Is at least one body dynamic?
            if (!bodyB.ShouldCollide(bodyA))
            {
                return;
            }

            // Check user filtering.
            if (m_contactFilter != null && !m_contactFilter.ShouldCollide(fixtureA, fixtureB))
            {
                return;
            }

            // Call the factory.
            b2Contact c = b2Contact.Create(fixtureA, indexA, fixtureB, indexB);
            if (c == null)
            {
                return;
            }

            // Contact creation may swap fixtures.
            //fixtureA = c.FixtureA;
            //fixtureB = c.FixtureB;
            //indexA = c.m_indexA;
            //indexB = c.m_indexB;
            bodyA = c.FixtureA.Body;
            bodyB = c.FixtureB.Body;

            // Insert into the world.
            c.Prev = null;
            c.Next = m_contactList;
            if (m_contactList != null)
            {
                m_contactList.Prev = c;
            }
            m_contactList = c;

            // Connect to island graph.

            // Connect to body A
            c.NodeA.Contact = c;
            c.NodeA.Other = bodyB;

            c.NodeA.Prev = null;
            c.NodeA.Next = bodyA.ContactList;
            if (bodyA.ContactList != null)
            {
                bodyA.ContactList.Prev = c.NodeA;
            }
            bodyA.ContactList = c.NodeA;

            // Connect to body B
            c.NodeB.Contact = c;
            c.NodeB.Other = bodyA;

            c.NodeB.Prev = null;
            c.NodeB.Next = bodyB.ContactList;
            if (bodyB.ContactList != null)
            {
                bodyB.ContactList.Prev = c.NodeB;
            }
            bodyB.ContactList = c.NodeB;

            // Wake up the bodies
            bodyA.SetAwake(true);
            bodyB.SetAwake(true);

            ++m_contactCount;
        }
    }
}
