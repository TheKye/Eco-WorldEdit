// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.
namespace Eco.Mods.TechTree
{
	using Eco.Gameplay.Objects;
	using Eco.Gameplay.Wires;

	public partial class DoorObject : WorldObject, IWireContainer
	{
		public void WE_SetOpensOut(bool oo)
		{
			this.OpensOut = oo;
		}
	}
}
