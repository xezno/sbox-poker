# Mapping for Poker

## Quick Start

You can use `maps/prefabs/poker_template.vmap` as a template. Place that in your map and it'll contain everything you need.
Feel free to collapse the prefab and modify things as you see fit.

![image](https://user-images.githubusercontent.com/12881812/223462400-7d8764df-2576-48f1-9d2b-7c6b0d2c3ddd.png)

## Details

Required entities:
 - For each player (prefab `maps/prefabs/poker_seat.vmap`)
   + `info_seat` - A seat for a player to sit at.
   + `info_player_chip_spawn` - A spawn point for chips.
 - Poker table (prefab `maps/prefabs/poker_table.vmap`)
   + `info_community_card_spawn` - A spawn point for community cards.

### Poker Seat

The poker seat prefab contains two variables - `seatNumber` and `seatName`.
Generally the number should increase anticlockwise, starting at the dealer (0) and ending at the cutoff (5).

![image](https://user-images.githubusercontent.com/12881812/223463381-ae968d16-2d72-4a5c-9606-b98d2ddcd913.png)


### Poker Table

The poker table prefab contains a model alongside a spawner for community cards.
The model inside the template prefab is just a hammer mesh. You can use whatever you want though - props, hammer meshes, etc.
