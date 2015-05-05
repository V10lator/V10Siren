/*
 * Don't copy this file to your mod!
 * It might get splitted into a library later but right now
 * copying it is a save way to end the world as we know it.
 */
using System;
using ICities;

namespace V10CoreUtils
{
	public interface V10CoreMod : IUserMod
	{
		string realName { get; }
		
		string streamID { get; }
	}
}

