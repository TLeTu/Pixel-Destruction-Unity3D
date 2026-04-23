# Pixel Destruction Unity3D

## Hướng dẫn chạy project
- Unity version: 6000.3.13f1
- Chạy MainScene, dữ liệu levels, blocks được lưu trong Resources, Game sẽ tự load các level trong Resources

## Kiến trúc code

Hệ thống chia 3 nhóm:

- Managers: điều phối game.
- Controllers: xử lý hành vi đối tượng trong scene.
- ScriptableObjects: dữ liệu cấu hình level và block.

### Managers chính

- GameManager: state machine tổng (menu, play, upgrade, place weapon, win), điều hướng luồng game.
- LevelManager: spawn block theo thời gian, quản lý block/chunk, cleanup level.
- InputManager: nhận tap/click và gây damage lên block.
- ObstacleManager: spawn obstacle, đặt weapon, áp dụng upgrade cho weapon.
- ScoreManager: cộng điểm, check ngưỡng upgrade, check điều kiện win.
- UIManager: đổi panel theo state, cập nhật XP/score, xử lý nút UI.
- PoolManager: pool pixel để giảm instantiate/destroy.
- SaveManager: lưu level mở khóa bằng PlayerPrefs.

### Controllers chính

- PixelBlockController: quản lý lưới pixel, damage, tách chunk khi vỡ.
- PixelController: xử lý detached pixel rồi trả về pool.
- IWeaponController + SawController + ShredderController: logic tấn công và nâng cấp weapon.
- WeaponSlotController + UpgradeBtnController: click đặt weapon/chọn upgrade.

### ScriptableObjects

- BlockData: sprite + pixelHealth của block.
- LevelConfig: cấu hình level (target, threshold, block list, damage tap, obstacle, weapon).

### Luồng nhanh

1. GameManager load LevelConfig và bắt đầu level.
2. LevelManager spawn PixelBlock, ObstacleManager spawn obstacle.
3. Người chơi tap + weapon gây damage lên block.
4. ScoreManager theo dõi điểm -> đến ngưỡng thì vào upgrade.
5. Đạt target -> win, save progress, cleanup level.

## Hướng cải thiện project