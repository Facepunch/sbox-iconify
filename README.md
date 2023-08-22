# sbox-iconify

😀 A simple way to use many different icon packs in a s&box project.

## Usage

```html
<iconify icon="ic:baseline-add-location" />
```

Full list of icons at [icones](https://icones.js.org/).

## More Info

Icons are fetched at runtime from the [iconify](https://iconify.design/) API.
These get cached in `FileSystem.Data` (JSON data alongside the SVG) so we don't have to keep requesting it.

