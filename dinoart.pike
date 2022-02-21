//Named by DeviCat: "Dino Art"!

int main(int argc, array(string) argv) {
	if (argc < 2) return 1;
	Image.Image texture = Image.Image(128, 128);
	string data = Stdio.read_file("substitute.png");
	if (!data) {
		data = Protocols->HTTP->get_url_data("https://imgs.xkcd.com/comics/substitute.png"); //-> not . to avoid unnecessary warnings
		Stdio.write_file("substitute.png", data);
	}
	Image.Image subst = Image.PNG.decode(data);
	Image.Image raptor = subst->copy(363, 431, 363+76, 431+42);
	texture->paste(raptor->rotate_ccw(), 77, 17);
	//texture->box(7, 18, 18, 42, 0, 255, 0); //Top block: top and back surfaces
	//texture->box(19, 18, 24, 41, 255, 255, 0); //Top block: bottom surface
	texture->box(25, 18, 33, 41, 255, 0, 255); //Top block: front surface. Wants "XKCD" credit.
	//texture->box(7, 42, 33, 50, 0, 255, 255);
	//texture->box(5, 73, 37, 122, 0, 0, 255); //Only visible on the back
	texture->box(0, 0, 6, 128, 255, 0, 0); //The edges of the device
	Stdio.write_file(argv[1], Image.PNG.encode(texture));
}
